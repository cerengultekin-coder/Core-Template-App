using CoreApp.Application.Common.Interfaces;
using MediatR;

namespace CoreApp.Application.Common.Behaviors;

public interface IRequireAuthenticatedUser { }

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICurrentUser _current;
    public AuthorizationBehavior(ICurrentUser current) => _current = current;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IRequireAuthenticatedUser && !_current.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await next();
    }
}
