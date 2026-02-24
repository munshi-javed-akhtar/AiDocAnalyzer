namespace Application.Common.Interfaces;

public interface IVectorDbService
{
    Task<bool> IsHealthyAsync();
    Task EnsureCollectionExistsAsync(string collectionName, uint vectorSize);
    Task UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points);
    Task<IEnumerable<SearchResult>> SearchAsync(string collectionName, float[] queryVector, int topK = 3);
    Task DeleteByDocumentIdAsync(string collectionName, Guid documentId);
}

public record VectorPoint(Guid Id, float[] Vector, Dictionary<string, string> Payload);

public record SearchResult(Guid Id, float Score, Dictionary<string, string> Payload);
