using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("test")]
[Tags("Testing")]
public class TestController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public TestController(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    /// <summary>Test embedding generation - returns vector length</summary>
    [HttpPost("embed")]
    public async Task<IActionResult> TestEmbed([FromBody] TestEmbedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text cannot be empty.");

        var vector = await _embeddingService.GenerateEmbeddingAsync(request.Text);
        return Ok(new EmbedResponse(vector.Length, "nomic-embed-text"));
    }
}

public record TestEmbedRequest(string Text);
