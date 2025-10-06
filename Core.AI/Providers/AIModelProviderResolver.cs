using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Microsoft.Extensions.DependencyInjection;

namespace Core.AI.Providers;

public class AIModelProviderResolver
{
    private readonly IServiceProvider _provider;

    public AIModelProviderResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IAIModelProvider Resolve(AIProvider provider)
    {
        return provider switch
        {
            AIProvider.Ollama => _provider.GetRequiredService<OllamaModelProvider>(),
            _ => _provider.GetRequiredService<OpenRouterModelProvider>()
        };
    }
}
