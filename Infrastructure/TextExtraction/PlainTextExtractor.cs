using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.TextExtraction;

public class PlainTextExtractor : ITextExtractor
{
    private readonly ILogger<PlainTextExtractor> _logger;

    public PlainTextExtractor(ILogger<PlainTextExtractor> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(string contentType) =>
        contentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("text/markdown", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        _logger.LogInformation("Extracting text from plain text file: {FileName}", fileName);
        using var reader = new StreamReader(fileStream);
        return await reader.ReadToEndAsync();
    }
}
