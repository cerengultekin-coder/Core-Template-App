using Core.AI.Abstractions;
using Core.AI.Models;
using Microsoft.Extensions.Configuration;

namespace Core.AI.Services;

public class AIModelConfigProvider : IAIModelConfigProvider
{
    private readonly IConfiguration _configuration;

    public AIModelConfigProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<AIModelConfig> GetConfigAsync(string modelNameOrAlias)
    {
        var section = _configuration.GetSection($"AIModels:{modelNameOrAlias}");

        if (!section.Exists())
            throw new InvalidOperationException($"Model '{modelNameOrAlias}' not found in configuration.");

        var config = section.Get<AIModelConfig>();
        if (config is null)
            throw new InvalidOperationException($"Model config for '{modelNameOrAlias}' is invalid.");

        return Task.FromResult(config);
    }
}
