using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.FunctionCalling.FunctionSchema;
using Core.AI.Models;

namespace Core.AI.Providers.Ollama;

public class OllamaAiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly OllamaSettings _ollama;
    private readonly IAiFunctionDispatcher _dispatcher;
    private readonly IFunctionRegistry _registry;

    public OllamaAiService(
        HttpClient httpClient,
        AISettings settings,
        OllamaSettings ollama,
        IAiFunctionDispatcher dispatcher,
        IFunctionRegistry registry)
    {
        _httpClient = httpClient;
        _settings = settings;
        _ollama = ollama;
        _dispatcher = dispatcher;
        _registry = registry;
    }

    public async Task<string> GetCompletionAsync(
        string prompt,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messages = BuildMessages(prompt, options?.SystemPrompt);

        var tools = options?.UseFunctionCalling == true
            ? FunctionSchemaGenerator.ToOllamaSchemas(_registry.GetAll())
            : null;

        var requestBody = new
        {
            model = options?.Model ?? _settings.Model,
            messages,
            tools
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_ollama.BaseUrl}/api/chat", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("message", out var message) &&
            message.TryGetProperty("tool_calls", out var toolCalls))
        {
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var fn = toolCall.GetProperty("function");
                var functionName = fn.GetProperty("name").GetString();
                var argsJson = fn.GetProperty("arguments").GetRawText();

                var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson)
                           ?? new Dictionary<string, object?>();

                var dispatch = await _dispatcher.TryDispatchAsync(functionName!, args, cancellationToken);

                var aiResponse = new AIResponse
                {
                    Content = !string.IsNullOrWhiteSpace(dispatch.Result)
                        ? dispatch.Result
                        : $"Function {functionName} executed but returned no result.",
                    FunctionExecuted = true,
                    FunctionName = functionName,
                    FunctionResult = !string.IsNullOrWhiteSpace(dispatch.Result)
                        ? JsonDocument.Parse(dispatch.Result!).RootElement.Clone()
                        : JsonDocument.Parse("{}").RootElement
                };

                return JsonSerializer.Serialize(aiResponse);
            }
        }

        if (root.TryGetProperty("message", out var msg) &&
            msg.TryGetProperty("content", out var content))
        {
            var aiResponse = new AIResponse
            {
                Content = content.GetString(),
                FunctionExecuted = false
            };

            return JsonSerializer.Serialize(aiResponse);
        }

        return JsonSerializer.Serialize(new AIResponse
        {
            Content = "[No response from Ollama]",
            FunctionExecuted = false
        });
    }

    public async Task<string> PromptAsync(
        string prompt,
        AIRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var fullPrompt = !string.IsNullOrWhiteSpace(options?.SystemPrompt)
            ? $"{options.SystemPrompt}\n\n{prompt}"
            : prompt;

        var body = new
        {
            model = options?.Model ?? _settings.Model,
            prompt = fullPrompt,
            stream = false
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_ollama.BaseUrl}/api/generate", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
    }

    public async IAsyncEnumerable<string> StreamPromptAsync(
        string prompt,
        AIRequestOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fullPrompt = !string.IsNullOrWhiteSpace(options?.SystemPrompt)
            ? $"{options.SystemPrompt}\n\n{prompt}"
            : prompt;

        var body = new
        {
            model = options?.Model ?? _settings.Model,
            prompt = fullPrompt,
            stream = true
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_ollama.BaseUrl}/api/generate", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line)) continue;

            string? value = null;
            try
            {
                using var doc = JsonDocument.Parse(line);

                if (doc.RootElement.TryGetProperty("response", out var resp))
                    value = resp.GetString();
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(value))
                yield return value!;
        }
    }

    public async Task<bool> IsModelSupportedAsync(string model)
    {
        using var res = await _httpClient.GetAsync($"{_ollama.BaseUrl}/api/tags");
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("models", out var models))
        {
            foreach (var m in models.EnumerateArray())
            {
                var name = m.GetProperty("name").GetString();
                if (string.Equals(name, model, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private static List<object> BuildMessages(string prompt, string? systemPrompt)
    {
        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });

        messages.Add(new { role = "user", content = prompt });

        return messages;
    }
}
