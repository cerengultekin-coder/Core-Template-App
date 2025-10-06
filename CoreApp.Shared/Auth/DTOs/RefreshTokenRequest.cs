namespace CoreApp.Shared.Auth.DTOs;

public record RefreshTokenRequest(string RefreshToken, string? SessionId);