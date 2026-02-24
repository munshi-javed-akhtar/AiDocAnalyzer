namespace Domain.Entities;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int ChunkCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}

public enum DocumentStatus
{
    Pending,
    Processing,
    Indexed,
    Failed
}
