using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Documents.Commands;

public record UploadDocumentCommand(Stream FileStream, string FileName, string ContentType, long FileSize) : IRequest<UploadDocumentResponse>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private const string CollectionName = "documents";
    private readonly IDocumentRepository _repository;
    private readonly ITextExtractor _textExtractor;
    private readonly ITextChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDbService _vectorDbService;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentRepository repository,
        ITextExtractor textExtractor,
        ITextChunker chunker,
        IEmbeddingService embeddingService,
        IVectorDbService vectorDbService,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _repository = repository;
        _textExtractor = textExtractor;
        _chunker = chunker;
        _embeddingService = embeddingService;
        _vectorDbService = vectorDbService;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing upload for {FileName}", request.FileName);

        // Save document record
        var document = new Document
        {
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSize,
            Status = DocumentStatus.Processing
        };
        await _repository.CreateAsync(document);

        try
        {
            // Extract text
            var text = await _textExtractor.ExtractTextAsync(request.FileStream, request.FileName);
            _logger.LogInformation("Extracted {Length} characters from {FileName}", text.Length, request.FileName);

            // Chunk text
            var chunks = _chunker.Chunk(text).ToList();
            _logger.LogInformation("Created {Count} chunks", chunks.Count);

            // Ensure vector collection exists
            await _vectorDbService.EnsureCollectionExistsAsync(CollectionName, (uint)_embeddingService.VectorSize);

            // Embed and store each chunk
            var vectorPoints = new List<VectorPoint>();
            var documentChunks = new List<DocumentChunk>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkId = Guid.NewGuid();
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i]);

                vectorPoints.Add(new VectorPoint(
                    chunkId,
                    embedding,
                    new Dictionary<string, string>
                    {
                        ["text"] = chunks[i],
                        ["documentId"] = document.Id.ToString(),
                        ["fileName"] = request.FileName,
                        ["chunkIndex"] = i.ToString()
                    }
                ));

                documentChunks.Add(new DocumentChunk
                {
                    Id = chunkId,
                    DocumentId = document.Id,
                    Text = chunks[i],
                    ChunkIndex = i
                });

                if (cancellationToken.IsCancellationRequested) break;
            }

            await _vectorDbService.UpsertVectorsAsync(CollectionName, vectorPoints);
            await _repository.AddChunksAsync(documentChunks);

            // Update document status
            document.Status = DocumentStatus.Indexed;
            document.ChunkCount = chunks.Count;
            document.ProcessedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(document);

            _logger.LogInformation("Successfully indexed {FileName} with {Count} chunks", request.FileName, chunks.Count);

            return new UploadDocumentResponse(document.Id, request.FileName, chunks.Count, "Indexed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {FileName}", request.FileName);
            document.Status = DocumentStatus.Failed;
            await _repository.UpdateAsync(document);
            throw;
        }
    }
}
