using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document?> GetByIdAsync(Guid id) =>
        await _context.Documents.Include(d => d.Chunks).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Document>> GetAllAsync() =>
        await _context.Documents.OrderByDescending(d => d.CreatedAt).ToListAsync();

    public async Task UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc != null)
        {
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddChunksAsync(IEnumerable<DocumentChunk> chunks)
    {
        _context.DocumentChunks.AddRange(chunks);
        await _context.SaveChangesAsync();
    }
}
