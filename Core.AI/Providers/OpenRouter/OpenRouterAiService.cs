using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.FunctionCalling.FunctionSchema;
using Core.AI.Models;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.AI.Providers.OpenRouter;

public class OpenRouterAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly AISettings _aiSettings;
    private readonly AiFunctionDispatcher _dispatcher;
    private readonly IFunctionRegistry _registry;

    public OpenRouterAiService(
        HttpClient httpClient,
        OpenRouterSettings settings,
        AISettings aiSettings,
        AiFunctionDispatcher dispatcher,
        IFunctionRegistry registry
)
    {
        _httpClient = httpClient;
        _settings = settings;
        _aiSettings = aiSettings;
        _dispatcher = dispatcher;
        _registry = registry;
    }

    public Task<string> PromptAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);
        var body = new { model = options?.Model ?? _aiSettings.Model, messages };

        var response = await SendRequestAsync(body, cancellationToken: default);

        return ExtractMessage(response);
    }

    public async Task<string> GetCompletionAsync(string prompt, AIRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);

        var tools = options?.UseFunctionCalling == true
            ? FunctionSchemaGenerator.ToOpenRouterTools(_registry.GetAll())
            : null;

        var body = new
        {
            model = options?.Model ?? _aiSettings.Model,
            messages,
            tools,
            tool_choice = options?.UseFunctionCalling == true ? "auto" : null
        };

        var response = await SendRequestAsync(body, cancellationToken);
        using var doc = JsonDocument.Parse(response);
        var message = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

        if (options?.UseFunctionCalling == true && message.TryGetProperty("tool_calls", out var toolCalls))
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var fn = toolCall.GetProperty("function");
                var functionName = fn.GetProperty("name").GetString();
                var argsJson = fn.GetProperty("arguments").GetString();

                var args = JsonDocument.Parse(argsJson!).RootElement
                    .EnumerateObject()
                    .ToDictionary(
                        prop => prop.Name,
                        prop => prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => (object?)prop.Value.GetString(),
                            JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? i : prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            _ => prop.Value.GetRawText()
                        });

                var result = await _dispatcher.TryDispatchAsync(functionName!, args, cancellationToken);

                return JsonSerializer.Serialize(new AIResponse
                {
                    Content = result?.Result ?? $"Function `{functionName}` executed but returned no result.",
                    FunctionExecuted = true,
                    FunctionName = functionName,
                    FunctionResult = !string.IsNullOrWhiteSpace(result?.Result)
                        ? JsonDocument.Parse(result.Result!).RootElement.Clone()
                        : JsonDocument.Parse("{}").RootElement
                });
            }
        }

        return JsonSerializer.Serialize(new AIResponse
        {
            Content = message.GetProperty("content").GetString(),
            FunctionExecuted = false
        });
    }

    public async IAsyncEnumerable<string> StreamPromptAsync(
        string prompt,
        AIRequestOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);

        var body = new
        {
            model = options?.Model ?? _aiSettings.Model,
            messages,
            stream = true
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:")) continue;

            var jsonLine = line["data:".Length..].Trim();

            if (jsonLine == "[DONE]") yield break;
            string? content = null;

            try
            {
                using var doc = JsonDocument.Parse(jsonLine);
                content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta")
                    .GetProperty("content")
                    .GetString();
            }
            catch
            {}

            if (!string.IsNullOrWhiteSpace(content))
            {
                yield return content!;
            }
        }
    }

    public async Task<bool> IsModelSupportedAsync(string model)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://openrouter.ai/api/v1/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.TryGetProperty("data", out var models) &&
               models.EnumerateArray().Any(m =>
                   m.TryGetProperty("id", out var idProp) &&
                   string.Equals(idProp.GetString(), model, StringComparison.OrdinalIgnoreCase));
    }

    private static List<object> BuildMessages(string prompt, string? systemPrompt)
    {
        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });

        messages.Add(new { role = "user", content = prompt });

        return messages;
    }

    private async Task<string> SendRequestAsync(object body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string ExtractMessage(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}
