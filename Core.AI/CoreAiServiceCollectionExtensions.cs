using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.MCP;
using Core.AI.Providers.Ollama;
using Core.AI.Providers.OpenRouter;
using Core.AI.Providers.SemanticKernel;
using Core.AI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Core.AI;

public static class CoreAiServiceCollectionExtensions
{
    public static IServiceCollection AddCoreAi(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AISettings>(config.GetSection("AiSettings"));
        services.Configure<OllamaSettings>(config.GetSection("Ollama"));
        services.Configure<OpenRouterSettings>(config.GetSection("OpenRouter"));
        services.Configure<AiCatalogOptions>(config.GetSection("AiCatalog"));
        services.Configure<McpOptions>(config.GetSection("Mcp"));

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AISettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OllamaSettings>>().Value);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OpenRouterSettings>>().Value);

        services.AddSingleton<IAiCatalogService, AiCatalogService>();

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(c => c.AssignableTo<IAiFunction>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.AddSingleton<IFunctionRegistry, InMemoryFunctionRegistry>();
        services.AddScoped<AiFunctionDispatcher>();
        services.AddScoped<IAiFunctionDispatcher, LegacyDispatcherAdapter>();

        services.AddSingleton<IMcpClient, McpClient>();
        services.AddSingleton<IMcpToolAdapter, McpToolAdapter>();
        services.Decorate<IAiFunctionDispatcher, McpFunctionDispatcherProxy>();

        services.AddHttpClient<OpenRouterAiService>();
        services.AddHttpClient<OllamaAiService>();

        services.AddScoped<IAIService>(sp =>
        {
            var settings = sp.GetRequiredService<AISettings>();
            return settings.Provider == AIProvider.Ollama
                ? sp.GetRequiredService<OllamaAiService>()
                : sp.GetRequiredService<OpenRouterAiService>();
        });

        services.AddScoped<IAgentService, SemanticKernelAgentService>();

        return services;
    }
}
