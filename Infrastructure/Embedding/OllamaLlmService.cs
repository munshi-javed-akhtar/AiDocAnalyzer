using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Embedding;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<OllamaLlmService> _logger;

    public OllamaLlmService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:LlmModel"] ?? "llama3";
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(string context, string question)
    {
        var prompt = $"""
            You are a document analysis AI. Answer ONLY using the provided context below.
            If the answer is not found in the context, say exactly: "Not found in document."
            Do not make up information. Be concise and accurate.

            Context:
            {context}

            Question: {question}

            Answer:
            """;

        var request = new
        {
            model = _model,
            prompt,
            stream = false
        };

        _logger.LogInformation("Sending RAG prompt to Ollama (model: {Model})", _model);

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseJson);

        return result?.response?.Trim() ?? "No response from LLM.";
    }

    private record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? response
    );
}
