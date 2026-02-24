using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Embedding;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OllamaEmbeddingService> _logger;
    public int VectorSize => 768; // nomic-embed-text

    public OllamaEmbeddingService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new { model = _model, prompt = text };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Generating embedding for text of length {Length}", text.Length);

        var response = await _httpClient.PostAsync("/api/embeddings", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseJson);

        if (result?.embedding == null || result.embedding.Length == 0)
            throw new InvalidOperationException("Empty embedding returned from Ollama");

        return result.embedding;
    }

    private record OllamaEmbeddingResponse(float[] embedding);
}
