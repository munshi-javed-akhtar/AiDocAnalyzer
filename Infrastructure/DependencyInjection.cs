using Application.Common.Interfaces;
using Infrastructure.Embedding;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.TextExtraction;
using Infrastructure.VectorDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Qdrant
        var qdrantHost = configuration["Qdrant:Host"] ?? "localhost";
        var qdrantPort = int.Parse(configuration["Qdrant:Port"] ?? "6333");
        services.AddSingleton(_ => new QdrantClient(qdrantHost, qdrantPort));
        services.AddScoped<IVectorDbService, QdrantVectorDbService>();

        // Ollama Embedding
        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(120);
        });

        // Ollama LLM
        services.AddHttpClient<ILlmService, OllamaLlmService>(client =>
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(300);
        });

        // Text Extraction
        services.AddScoped<PdfTextExtractor>();
        services.AddScoped<PlainTextExtractor>();
        services.AddScoped<ITextExtractor>(sp =>
        {
            var extractors = new List<ITextExtractor>
            {
                sp.GetRequiredService<PdfTextExtractor>(),
                sp.GetRequiredService<PlainTextExtractor>()
            };
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TextExtractorFactory>>();
            return new TextExtractorFactory(extractors, logger);
        });

        // Chunker
        services.AddScoped<ITextChunker, TextChunker>();

        // Repository
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }
}
