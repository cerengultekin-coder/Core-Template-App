using System.Security.Claims;
using System.Text;
using Core.AI.Abstractions;
using Core.AI.Commands;
using Core.AI.Config;
using Core.AI.Models.Agent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[Route("api/ai")]
[ApiController]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIService _aiService;
    private readonly IAgentService _agentService;
    private readonly IAiCatalogService _catalog;

    public AiController(
        IMediator mediator,
        IAIService aiService,
        IAgentService agentService,
        IAiCatalogService catalog)
    {
        _mediator = mediator;
        _aiService = aiService;
        _agentService = agentService;
        _catalog = catalog;
    }

    [HttpGet("providers")]
    [AllowAnonymous]
    public IActionResult Providers() => Ok(_catalog.GetProviders());

    [HttpGet("models")]
    [AllowAnonymous]
    public IActionResult Models([FromQuery] string provider) => Ok(_catalog.GetModels(provider));

    [HttpGet("defaults")]
    [AllowAnonymous]
    public IActionResult Defaults() => Ok(new { provider = _catalog.GetDefaultProvider(), model = _catalog.GetDefaultModel() });

    [HttpPost("prompt")]
    public async Task<IActionResult> Prompt([FromBody] PromptTextCommand command)
        => Ok(new { result = await _mediator.Send(command) });

    [HttpPost("completion")]
    public async Task<IActionResult> Completion([FromBody] PromptTextCommand command)
        => Ok(new { result = await _aiService.GetCompletionAsync(command.Prompt) });

    [HttpGet("model-supported")]
    [AllowAnonymous]
    public async Task<IActionResult> IsModelSupported([FromQuery] string model)
        => Ok(new { model, isSupported = await _aiService.IsModelSupportedAsync(model) });

    [HttpPost("stream")]
    public async Task StreamPrompt([FromBody] PromptTextCommand command)
    {
        Response.ContentType = "text/plain";
        await foreach (var chunk in _aiService.StreamPromptAsync(command.Prompt, command.Options))
        {
            var buffer = Encoding.UTF8.GetBytes(chunk);
            await Response.Body.WriteAsync(buffer);
            await Response.Body.FlushAsync();
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AiChatRequest request)
    {
        var opts = new AgentRequestOptions
        {
            AgentId = request.AgentId,
            Profile = request.AgentId,
            Model = request.Model,
            UseFunctionCalling = request.UseFunctionCalling,
            Provider = Enum.TryParse<AIProvider>(request.Provider, out var p) ? p : null
        };

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? request.UserId;
        var response = await _agentService.ChatAsync(request.Prompt, opts, userId);
        return Ok(new { Content = response });
    }

    public class AiChatRequest
    {
        public string Prompt { get; set; } = "";
        public string Provider { get; set; } = "OpenRouter";
        public string Model { get; set; } = "mistralai/mistral-small-3.2-24b-instruct:free";
        public bool UseFunctionCalling { get; set; } = true;
        public string? AgentId { get; set; }
        public string? UserId { get; set; }
    }

    private string? GetUserIdFromClaims()
    {
        var u = HttpContext.User;
        return u?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? u?.FindFirst("sub")?.Value
            ?? u?.FindFirst("uid")?.Value;
    }

}
