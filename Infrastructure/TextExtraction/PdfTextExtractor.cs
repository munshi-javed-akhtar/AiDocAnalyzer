using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Infrastructure.TextExtraction;

public class PdfTextExtractor : ITextExtractor
{
    private readonly ILogger<PdfTextExtractor> _logger;

    public PdfTextExtractor(ILogger<PdfTextExtractor> logger)
    {
        _logger = logger;
    }

    public bool CanHandle(string contentType) =>
        contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        _logger.LogInformation("Extracting text from PDF: {FileName}", fileName);

        // Copy stream to memory (PdfPig requires seekable stream)
        using var ms = new MemoryStream();
        fileStream.CopyTo(ms);
        ms.Position = 0;

        using var document = PdfDocument.Open(ms.ToArray());
        var sb = new StringBuilder();

        foreach (Page page in document.GetPages())
        {
            var pageText = string.Join(" ", page.GetWords().Select(w => w.Text));
            sb.AppendLine(pageText);
        }

        var result = sb.ToString().Trim();
        _logger.LogInformation("Extracted {Length} characters from {PageCount} pages", result.Length, document.NumberOfPages);

        return Task.FromResult(result);
    }
}
