using System.Security.Claims;
using System.Text;
using Core.AI.Abstractions;
using Core.AI.Models.Agent;
using Core.AI.Providers.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[Route("api/agent")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly AgentProfileProvider _profiles;

    public AgentController(IAgentService agentService, AgentProfileProvider profiles)
    {
        _agentService = agentService;
        _profiles = profiles;
    }

    [HttpGet("profiles")]
    [AllowAnonymous]
    public IActionResult GetProfiles()
        => Ok(_profiles.GetAllProfiles().Select(p => new { p.Id, p.Name, p.Description }));

    [HttpPost("prompt")]
    [Authorize]
    public async Task<IActionResult> Prompt([FromBody] AgentPromptRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _agentService.ChatAsync(request.Prompt, request.Options, userId);
        return Ok(new { result });
    }

    [HttpPost("stream")]
    [Authorize]
    public async Task StreamPrompt([FromBody] AgentPromptRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (string.IsNullOrWhiteSpace(userId))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Response.ContentType = "text/plain";
        await foreach (var chunk in _agentService.StreamChatAsync(request.Prompt, request.Options, userId))
        {
            var buffer = Encoding.UTF8.GetBytes(chunk);
            await Response.Body.WriteAsync(buffer);
            await Response.Body.FlushAsync();
        }
    }

    private string? GetUserIdFromClaims()
    {
        var u = HttpContext.User;
        return u?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? u?.FindFirst("sub")?.Value
            ?? u?.FindFirst("uid")?.Value;
    }
}
