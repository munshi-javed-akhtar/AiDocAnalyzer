namespace Application.Common.Interfaces;

public interface ILlmService
{
    Task<string> GenerateAnswerAsync(string context, string question);
}
