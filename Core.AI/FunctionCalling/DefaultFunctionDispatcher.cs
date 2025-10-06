namespace Core.AI.FunctionCalling;

public sealed class DefaultFunctionDispatcher : IAiFunctionDispatcher
{
    private readonly IFunctionRegistry _registry;

    public DefaultFunctionDispatcher(IFunctionRegistry registry)
    {
        _registry = registry;
    }

    public async Task<FunctionDispatchResult> TryDispatchAsync(
        string functionName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken ct)
    {
        if (_registry.TryGet(functionName, out var fn))
        {
            var resultJson = await fn.InvokeAsync(arguments, ct);
            return new FunctionDispatchResult { Handled = true, Result = resultJson ?? "{}" };
        }

        return new FunctionDispatchResult { Handled = false, Result = null };
    }
}
