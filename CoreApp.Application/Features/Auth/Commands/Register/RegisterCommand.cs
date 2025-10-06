using CoreApp.Shared.Auth.DTOs;
using MediatR;

namespace CoreApp.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;
