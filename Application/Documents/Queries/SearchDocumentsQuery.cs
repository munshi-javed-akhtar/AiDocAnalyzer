using Application.Common.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Documents.Queries;

public record SearchDocumentsQuery(string Query, int TopK = 3) : IRequest<SearchResponse>;

public class SearchDocumentsQueryHandler : IRequestHandler<SearchDocumentsQuery, SearchResponse>
{
    private const string CollectionName = "documents";
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDbService _vectorDbService;
    private readonly ILogger<SearchDocumentsQueryHandler> _logger;

    public SearchDocumentsQueryHandler(
        IEmbeddingService embeddingService,
        IVectorDbService vectorDbService,
        ILogger<SearchDocumentsQueryHandler> logger)
    {
        _embeddingService = embeddingService;
        _vectorDbService = vectorDbService;
        _logger = logger;
    }

    public async Task<SearchResponse> Handle(SearchDocumentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching for: {Query}", request.Query);

        var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.Query);
        var results = await _vectorDbService.SearchAsync(CollectionName, queryVector, request.TopK);

        var chunks = results.Select(r => new SearchChunk(
            r.Id,
            Guid.Parse(r.Payload.GetValueOrDefault("documentId", Guid.Empty.ToString())!),
            r.Payload.GetValueOrDefault("text", string.Empty)!,
            r.Score
        )).ToList();

        var combined = string.Join("\n\n---\n\n", chunks.Select(c => c.Text));

        return new SearchResponse(chunks, combined);
    }
}
