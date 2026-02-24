namespace Application.Common.Interfaces;

public interface ITextChunker
{
    IEnumerable<string> Chunk(string text, int chunkSize = 600, int overlap = 100);
}
