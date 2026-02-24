using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.TextExtraction;

public class TextExtractorFactory : ITextExtractor
{
    private readonly IEnumerable<ITextExtractor> _extractors;
    private readonly ILogger<TextExtractorFactory> _logger;

    public TextExtractorFactory(IEnumerable<ITextExtractor> extractors, ILogger<TextExtractorFactory> logger)
    {
        _extractors = extractors;
        _logger = logger;
    }

    public bool CanHandle(string contentType) => true;

    public Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        // Determine content type from filename if needed
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            _ => "text/plain"
        };

        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(contentType))
            ?? throw new NotSupportedException($"No extractor found for file type: {ext}");

        return extractor.ExtractTextAsync(fileStream, fileName);
    }
}
