using Core.AI.Models;

namespace Core.AI.Abstractions;
public interface IAIService
{
    Task<string> PromptAsync(string prompt, AIRequestOptions? options = default, CancellationToken cancellationToken = default);
    Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = default, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamPromptAsync(string prompt, AIRequestOptions? options = default, CancellationToken cancellationToken = default);
    Task<bool> IsModelSupportedAsync(string model);
}

