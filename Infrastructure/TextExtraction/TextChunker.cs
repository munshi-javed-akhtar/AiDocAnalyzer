using Application.Common.Interfaces;

namespace Infrastructure.TextExtraction;

public class TextChunker : ITextChunker
{
    public IEnumerable<string> Chunk(string text, int chunkSize = 600, int overlap = 100)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        // Normalize whitespace
        text = string.Join(" ", text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l)));

        int start = 0;
        while (start < text.Length)
        {
            int end = Math.Min(start + chunkSize, text.Length);

            // Try to break at sentence or word boundary
            if (end < text.Length)
            {
                int sentenceBreak = text.LastIndexOfAny(['.', '!', '?', '\n'], end, Math.Min(100, end - start));
                if (sentenceBreak > start)
                    end = sentenceBreak + 1;
                else
                {
                    int wordBreak = text.LastIndexOf(' ', end, Math.Min(50, end - start));
                    if (wordBreak > start)
                        end = wordBreak;
                }
            }

            var chunk = text[start..end].Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
                yield return chunk;

            start = Math.Max(start + 1, end - overlap);
        }
    }
}
