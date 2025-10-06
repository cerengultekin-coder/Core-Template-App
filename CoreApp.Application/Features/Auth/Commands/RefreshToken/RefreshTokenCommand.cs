using CoreApp.Shared.Auth.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenRequest Request) : IRequest<AuthResponse>;
