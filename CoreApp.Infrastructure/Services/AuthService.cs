using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Domain.Entities;
using CoreApp.Infrastructure.Data;
using CoreApp.Shared.Auth;
using CoreApp.Shared.Auth.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CoreApp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly CoreAppDbContext _ctx;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenSvc;

    public AuthService(CoreAppDbContext ctx, IPasswordHasher hasher, ITokenService tokenSvc)
        => (_ctx, _hasher, _tokenSvc) = (ctx, hasher, tokenSvc);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, string? sessionId, string? ip, string? ua)
    {
        if (await _ctx.Users.AnyAsync(u => u.Email == req.Email))
            throw new InvalidOperationException("User already exists");

        var user = new User(req.Username, req.Email, _hasher.Hash(req.Password));
        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync();

        return await IssueAsync(user, sessionId, ip, ua);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, string? sessionId, string? ip, string? ua)
    {
        var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Email == req.Email)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!_hasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return await IssueAsync(user, sessionId, ip, ua);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? sessionId, string? ip, string? ua)
    {
        var hash = TokenHelpers.Sha256Base64Url(TokenHelpers.CleanBearer(refreshToken));

        var token = await _ctx.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash && !rt.IsRevoked);

        if (token is null || token.Expires <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        if (!string.IsNullOrWhiteSpace(sessionId) && token.SessionId != sessionId)
            throw new UnauthorizedAccessException("Refresh token session mismatch");

        token.IsRevoked = true;
        var resp = await IssueAsync(token.User!, sessionId, ip, ua);
        token.ReplacedByTokenHash = TokenHelpers.Sha256Base64Url(resp.RefreshToken);
        await _ctx.SaveChangesAsync();

        return resp;
    }

    private async Task<AuthResponse> IssueAsync(User user, string? sessionId, string? ip, string? ua)
    {
        var access = _tokenSvc.GenerateAccessToken(user);
        var (raw, entity) = _tokenSvc.GenerateRefreshToken(user, sessionId, ip, ua);

        _ctx.RefreshTokens.Add(entity);
        await _ctx.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = access,
            RefreshToken = raw,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
    }
}
