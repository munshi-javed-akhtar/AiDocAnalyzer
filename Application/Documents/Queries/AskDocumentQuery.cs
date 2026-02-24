using Application.Common.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Documents.Queries;

public record AskDocumentQuery(string Question, int TopK = 3) : IRequest<AskResponse>;

public class AskDocumentQueryHandler : IRequestHandler<AskDocumentQuery, AskResponse>
{
    private const string CollectionName = "documents";
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDbService _vectorDbService;
    private readonly ILlmService _llmService;
    private readonly ILogger<AskDocumentQueryHandler> _logger;

    public AskDocumentQueryHandler(
        IEmbeddingService embeddingService,
        IVectorDbService vectorDbService,
        ILlmService llmService,
        ILogger<AskDocumentQueryHandler> logger)
    {
        _embeddingService = embeddingService;
        _vectorDbService = vectorDbService;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<AskResponse> Handle(AskDocumentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing question: {Question}", request.Question);

        // Retrieve relevant chunks
        var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.Question);
        var results = await _vectorDbService.SearchAsync(CollectionName, queryVector, request.TopK);

        var chunks = results.Select(r => new SearchChunk(
            r.Id,
            Guid.Parse(r.Payload.GetValueOrDefault("documentId", Guid.Empty.ToString())!),
            r.Payload.GetValueOrDefault("text", string.Empty)!,
            r.Score
        )).ToList();

        if (!chunks.Any())
        {
            return new AskResponse("No relevant documents found to answer your question.", request.Question, chunks);
        }

        var context = string.Join("\n\n---\n\n", chunks.Select(c => c.Text));
        var answer = await _llmService.GenerateAnswerAsync(context, request.Question);

        return new AskResponse(answer, request.Question, chunks);
    }
}
