using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task<Document> CreateAsync(Document document);
    Task<Document?> GetByIdAsync(Guid id);
    Task<IEnumerable<Document>> GetAllAsync();
    Task UpdateAsync(Document document);
    Task DeleteAsync(Guid id);
    Task AddChunksAsync(IEnumerable<DocumentChunk> chunks);
}
