namespace Core.AI.Abstractions;

public interface IAIModelProvider
{
    Task<List<string>> GetAvailableModelsAsync();
}
