using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Microsoft.Extensions.Options;
using System.Threading;

namespace Core.AI.Providers;

public class AIServiceResolver(IOptions<AISettings> settings, OpenRouterAiService open, OllamaAiService ollama) : IAIService
{
    private readonly AISettings _settings = settings.Value;
    private readonly OpenRouterAiService _openRouter = open;
    private readonly OllamaAiService _ollama = ollama;

    private IAIService Resolve(AIProvider? overrideProvider)
    {
        return overrideProvider switch
        {
            AIProvider.Ollama => _ollama,
            AIProvider.OpenRouter => _openRouter,
            null => _settings.Provider == AIProvider.Ollama ? _ollama : _openRouter
        };
    }

    public Task<string> PromptAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
        => Resolve(options?.Provider).PromptAsync(prompt, options, cancellationToken);

    public IAsyncEnumerable<string> StreamPromptAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
        => Resolve(options?.Provider).StreamPromptAsync(prompt, options, cancellationToken);

    public Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
        => Resolve(options?.Provider).GetCompletionAsync(prompt, options, cancellationToken);

    public Task<bool> IsModelSupportedAsync(string model)
        => Resolve(null).IsModelSupportedAsync(model);
}
