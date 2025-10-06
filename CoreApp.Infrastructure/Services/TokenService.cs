using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoreApp.Application.Common.Interfaces.Auth;
using CoreApp.Application.Common.Settings;
using CoreApp.Domain.Entities;
using CoreApp.Shared.Auth;
using Microsoft.IdentityModel.Tokens;

namespace CoreApp.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    public TokenService(JwtSettings jwt) => _jwt = jwt;

    public string GenerateAccessToken(User user)
    {
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now.AddSeconds(-30),
            expires: now.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string rawRefresh, RefreshToken entity) GenerateRefreshToken(User user, string? sessionId, string? ip, string? ua)
    {
        var raw = TokenHelpers.Base64Url(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            TokenHash = TokenHelpers.Sha256Base64Url(raw),
            Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            SessionId = sessionId,
            IpAddress = ip,
            UserAgent = ua,
            UserId = user.Id
        };
        return (raw, entity);
    }
}
