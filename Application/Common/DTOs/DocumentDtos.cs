namespace Application.Common.DTOs;

public record UploadDocumentResponse(
    Guid DocumentId,
    string FileName,
    int ChunkCount,
    string Status
);

public record SearchRequest(string Query, int TopK = 3);

public record SearchResponse(
    IEnumerable<SearchChunk> Chunks,
    string CombinedContext
);

public record SearchChunk(
    Guid ChunkId,
    Guid DocumentId,
    string Text,
    float Score
);

public record AskRequest(string Question, int TopK = 3);

public record AskResponse(
    string Answer,
    string Question,
    IEnumerable<SearchChunk> Sources
);

public record EmbedResponse(int VectorLength, string Model);
