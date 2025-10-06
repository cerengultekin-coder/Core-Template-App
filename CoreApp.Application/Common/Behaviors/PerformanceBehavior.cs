using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreApp.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _sw = new();

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _sw.Restart();
        var response = await next();
        _sw.Stop();

        if (_sw.ElapsedMilliseconds > 500)
            _logger.LogWarning("Long running request {Request} took {Elapsed} ms. Payload: {@Payload}",
                typeof(TRequest).Name, _sw.ElapsedMilliseconds, request);

        return response;
    }
}
