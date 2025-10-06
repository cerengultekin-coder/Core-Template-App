using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Models;
using Microsoft.Extensions.Options;

namespace Core.AI.Services;

public class AiCatalogService : IAiCatalogService
{
    private readonly List<AIModel> _models;
    private readonly AISettings _aiSettings;

    public AiCatalogService(IOptions<AiCatalogOptions> catalog, AISettings aiSettings)
    {
        _models = catalog.Value.Models ?? new();
        _aiSettings = aiSettings;
    }

    public string GetDefaultProvider()
    {
        return _models.FirstOrDefault(m => m.IsDefault)?.Provider
            ?? _aiSettings.Provider?.ToString()
            ?? "OpenRouter";
    }

    public string GetDefaultModel()
    {
        return _models.FirstOrDefault(m => m.IsDefault)?.Name
            ?? _aiSettings.Model
            ?? "mistralai/mistral-small-3.2-24b-instruct:free";
    }

    public IEnumerable<string> GetProviders()
        => _models.Select(m => m.Provider).Distinct();

    public IEnumerable<string> GetModels(string provider)
        => _models
            .Where(m => m.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Name);

    public bool IsValidModel(string provider, string model)
        => _models.Any(m =>
            m.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) &&
            m.Name.Equals(model, StringComparison.OrdinalIgnoreCase));
}
