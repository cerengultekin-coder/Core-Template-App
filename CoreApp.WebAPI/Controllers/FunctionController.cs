using Core.AI.Abstractions;
using Core.AI.Config;
using Core.AI.FunctionCalling;
using Core.AI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FunctionController : ControllerBase
{
    private readonly IAIService _aiService;

    public FunctionController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("call")]
    public async Task<IActionResult> Call([FromBody] FunctionCallRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt cannot be empty.");

        AIProvider? provider = null;

        if (!string.IsNullOrWhiteSpace(request.Options?.Provider) &&
            Enum.TryParse<AIProvider>(request.Options.Provider, true, out var parsed))
        {
            provider = parsed;
        }

        var options = new AIRequestOptions
        {
            Model = request.Options?.Model,
            Provider = provider
        };

        var result = await _aiService.GetCompletionAsync(request.Prompt, options, cancellationToken);

        return Ok(result);
    }

}
