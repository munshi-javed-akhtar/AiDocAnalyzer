using Application.Common.DTOs;
using Application.Documents.Commands;
using Application.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("documents")]
[Tags("Documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Upload and index a PDF or text document</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var allowedTypes = new[] { "application/pdf", "text/plain", "text/markdown" };
        if (!allowedTypes.Any(t => file.ContentType.StartsWith(t, StringComparison.OrdinalIgnoreCase)))
            return BadRequest($"Unsupported file type: {file.ContentType}. Supported: PDF, TXT, MD");

        _logger.LogInformation("Upload request: {FileName} ({Size} bytes)", file.FileName, file.Length);

        await using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>Semantic search across indexed documents</summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query cannot be empty.");

        var query = new SearchDocumentsQuery(request.Query, request.TopK);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>Ask a question — full RAG pipeline (retrieval + LLM answer)</summary>
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question cannot be empty.");

        var query = new AskDocumentQuery(request.Question, request.TopK);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
