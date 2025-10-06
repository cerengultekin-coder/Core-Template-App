using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Shared.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CoreApp.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IAuthService _auth;
    private readonly IHttpContextAccessor _http;

    public RegisterCommandHandler(IAuthService auth, IHttpContextAccessor http)
        => (_auth, _http) = (auth, http);

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var http = _http.HttpContext;

        var sessionId = http?.Request.Cookies["ctai_session_id"]
                        ?? http?.Request.Headers["X-Session-Id"].FirstOrDefault();

        var ip = http?.Connection.RemoteIpAddress?.ToString();
        var ua = http?.Request.Headers["User-Agent"].ToString();

        return await _auth.RegisterAsync(request.Request, sessionId, ip, ua);
    }
}
