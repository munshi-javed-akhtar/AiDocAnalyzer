namespace Application.Common.Interfaces;

public interface ITextExtractor
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName);
    bool CanHandle(string contentType);
}
