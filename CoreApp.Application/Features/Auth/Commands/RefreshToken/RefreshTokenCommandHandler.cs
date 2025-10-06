using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Shared.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CoreApp.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IAuthService _auth;
    private readonly IHttpContextAccessor _http;

    public RefreshTokenCommandHandler(IAuthService auth, IHttpContextAccessor http)
        => (_auth, _http) = (auth, http);

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var http = _http.HttpContext;

        var sessionId = http?.Request.Cookies["ctai_session_id"]
                        ?? http?.Request.Headers["X-Session-Id"].FirstOrDefault();

        var ip = http?.Connection.RemoteIpAddress?.ToString();
        var ua = http?.Request.Headers["User-Agent"].ToString();

        return await _auth.RefreshTokenAsync(request.Request.RefreshToken, sessionId, ip, ua);
    }
}
