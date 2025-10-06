using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.Memory;
using Core.AI.Models;
using Core.AI.Models.Agent;
using Core.AI.Providers.Profiles;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Core.AI.Providers.SemanticKernel;

public class SemanticKernelAgentService : IAgentService
{
    private readonly ChatHistoryStore _chatStore;
    private readonly AgentProfileProvider _profileProvider;
    private readonly AgentRequestOptionsValidator _validator;
    private readonly IConfiguration _config;

    public SemanticKernelAgentService(
        IConfiguration config,
        ChatHistoryStore chatStore,
        AgentProfileProvider profileProvider)
    {
        _config = config;
        _chatStore = chatStore;
        _profileProvider = profileProvider;
        _validator = new AgentRequestOptionsValidator();
    }

    private (Kernel Kernel, IChatCompletionService ChatService) BuildKernel(AIRequestOptions? options)
    {
        var defaultProvider = Enum.TryParse(_config["AiSettings:Provider"], out AIProvider fb) ? fb : AIProvider.OpenRouter;
        var provider = options?.Provider ?? defaultProvider;
        var model = options?.Model ?? _config["AiSettings:Model"];
        var b = Kernel.CreateBuilder();

        switch (provider)
        {
            case AIProvider.OpenRouter:
                var key = _config["OpenRouter:ApiKey"];
                b.AddOpenAIChatCompletion(modelId: model, apiKey: key, serviceId: "openrouter", endpoint: new Uri("https://openrouter.ai/api/v1"));
                break;
            case AIProvider.Ollama:
                b.AddOpenAIChatCompletion(modelId: model, apiKey: null, serviceId: "ollama", endpoint: new Uri("http://localhost:11434/v1"));
                break;
            default:
                throw new NotSupportedException($"Unsupported provider: {provider}");
        }

        var kernel = b.Build();
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        return (kernel, chat);
    }

    public async Task<string> ChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null)
    {
        options ??= new AgentRequestOptions();

        var validation = _validator.Validate(options);

        if (!validation.IsValid)
            return $"[Validation Error] {string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))}";

        var profile = _profileProvider.GetProfile(options.Profile ?? "multilingual");

        var context = !string.IsNullOrWhiteSpace(options.Context)
            ? options.Context
            : (!string.IsNullOrWhiteSpace(profile.SystemPrompt)
                ? profile.SystemPrompt
                : "You are a helpful assistant. Detect the user’s language and reply in that language.");

        var temperature = options.Temperature ?? profile.Temperature;

        var (_, chatService) = BuildKernel(options);

        var messages = new ChatHistory();
        messages.AddSystemMessage(context);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            foreach (var (role, content) in _chatStore.GetHistory(userId))
            {
                if (role == "user") messages.AddUserMessage(content);
                else if (role == "assistant") messages.AddAssistantMessage(content);
            }
            _chatStore.AddMessage(userId, "user", prompt);
        }

        messages.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings { Temperature = temperature };
        var response = await chatService.GetChatMessageContentAsync(messages, settings);
        var result = response?.Content ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
            _chatStore.AddMessage(userId, "assistant", result);

        return result;
    }

    public async IAsyncEnumerable<string> StreamChatAsync(string prompt, AgentRequestOptions? options = null, string? userId = null)
    {
        options ??= new AgentRequestOptions();

        var validation = _validator.Validate(options);

        if (!validation.IsValid)
        {
            yield return $"[Validation Error] {string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))}";
            yield break;
        }

        var profile = _profileProvider.GetProfile(options.Profile ?? "multilingual");

        var context = !string.IsNullOrWhiteSpace(options.Context)
            ? options.Context
            : (!string.IsNullOrWhiteSpace(profile.SystemPrompt)
                ? profile.SystemPrompt
                : "You are a helpful assistant. Detect the user’s language and reply in that language.");

        var temperature = options.Temperature ?? profile.Temperature;

        var (_, chatService) = BuildKernel(options);

        var messages = new ChatHistory();
        messages.AddSystemMessage(context);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            foreach (var (role, content) in _chatStore.GetHistory(userId))
            {
                if (role == "user") messages.AddUserMessage(content);
                else if (role == "assistant") messages.AddAssistantMessage(content);
            }
            _chatStore.AddMessage(userId, "user", prompt);
        }

        messages.AddUserMessage(prompt);

        var exec = new OpenAIPromptExecutionSettings { Temperature = temperature };
        var stream = chatService.GetStreamingChatMessageContentsAsync(messages, exec);

        string full = "";
        await foreach (var chunk in stream)
        {
            if (!string.IsNullOrWhiteSpace(chunk.Content))
            {
                yield return chunk.Content;
                full += chunk.Content;
            }
        }

        if (!string.IsNullOrWhiteSpace(userId))
            _chatStore.AddMessage(userId, "assistant", full);
    }

    public Task<bool> IsModelSupportedAsync(string model, string provider) => Task.FromResult(true);
}
