using System.Net.Http.Headers;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;

namespace Core.AI.Providers.OpenRouter;

public class OpenRouterModelProvider : IAIModelProvider
{
    private readonly HttpClient _httpClient;

    public OpenRouterModelProvider(OpenRouterSettings settings)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", settings.ApiKey);
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        var response = await _httpClient.GetAsync("models");
        var content = await response.Content.ReadAsStringAsync();

        var list = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(content);
            var models = doc.RootElement.GetProperty("data");
            foreach (var m in models.EnumerateArray())
            {
                var id = m.GetProperty("id").GetString();
                if (!string.IsNullOrWhiteSpace(id)) list.Add(id);
            }
        }
        catch
        {}

        return list;
    }
}
