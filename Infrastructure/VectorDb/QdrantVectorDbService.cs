using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Infrastructure.VectorDb;

public class QdrantVectorDbService : IVectorDbService
{
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantVectorDbService> _logger;

    public QdrantVectorDbService(QdrantClient client, ILogger<QdrantVectorDbService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            await _client.ListCollectionsAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Qdrant health check failed");
            return false;
        }
    }

 public async Task EnsureCollectionExistsAsync(string collectionName, uint vectorSize)
{
    var collections = await _client.ListCollectionsAsync();
    if (!collections.Any(c => c == collectionName))
    {
        _logger.LogInformation("Creating Qdrant collection: {Collection}", collectionName);
        await _client.CreateCollectionAsync(collectionName, new VectorParams
        {
            Size = vectorSize,
            Distance = Distance.Cosine
        });
    }
}

    public async Task UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points)
    {
        var qdrantPoints = points.Select(p =>
        {
            var point = new PointStruct
            {
                Id = new PointId { Uuid = p.Id.ToString() },
                Vectors = p.Vector
            };
            foreach (var kv in p.Payload)
                point.Payload[kv.Key] = kv.Value;
            return point;
        }).ToList();

        await _client.UpsertAsync(collectionName, qdrantPoints);
        _logger.LogInformation("Upserted {Count} vectors to {Collection}", qdrantPoints.Count, collectionName);
    }

    public async Task<IEnumerable<Application.Common.Interfaces.SearchResult>> SearchAsync(
        string collectionName, float[] queryVector, int topK = 3)
    {
        var results = await _client.SearchAsync(collectionName, queryVector, limit: (ulong)topK);

        return results.Select(r => new Application.Common.Interfaces.SearchResult(
            Guid.Parse(r.Id.Uuid),
            r.Score,
            r.Payload.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.StringValue ?? string.Empty)
        ));
    }

    public async Task DeleteByDocumentIdAsync(string collectionName, Guid documentId)
    {
        await _client.DeleteAsync(collectionName, new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "documentId",
                        Match = new Match { Text = documentId.ToString() }
                    }
                }
            }
        });
    }
}
