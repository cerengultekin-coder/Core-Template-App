using CoreApp.Shared.Auth.DTOs;

namespace CoreApp.Application.Common.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? sessionId, string? ip, string? ua);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? sessionId, string? ip, string? ua);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string? sessionId, string? ip, string? ua);
}