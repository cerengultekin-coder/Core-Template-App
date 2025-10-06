using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Shared.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var (ip, ua, sid) = Meta();
            var res = await _auth.RegisterAsync(request, sid, ip, ua);
            return Ok(res);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Already exists", Detail = ex.Message, Status = 409 });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var (ip, ua, sid) = Meta();
            var res = await _auth.LoginAsync(request, sid, ip, ua);
            return Ok(res);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = 401 });
        }
    }

    public record RefreshRequest(string RefreshToken, string? SessionId);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var (ip, ua, _) = Meta();
            var res = await _auth.RefreshTokenAsync(request.RefreshToken, request.SessionId, ip, ua);
            return Ok(res);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message, Status = 401 });
        }
    }

    private (string ip, string ua, string? sid) Meta()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();
        var sid = Request.Headers.TryGetValue("x-session-id", out var v) ? v.ToString() : null;
        return (ip, ua, sid);
    }
}
