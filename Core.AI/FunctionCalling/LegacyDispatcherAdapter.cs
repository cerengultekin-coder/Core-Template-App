using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.AI.FunctionCalling;

public sealed class LegacyDispatcherAdapter : IAiFunctionDispatcher
{
    private readonly AiFunctionDispatcher _inner;
    private readonly IFunctionRegistry _registry;

    public LegacyDispatcherAdapter(AiFunctionDispatcher inner, IFunctionRegistry registry)
    {
        _inner = inner;
        _registry = registry;
    }

    public async Task<FunctionDispatchResult> TryDispatchAsync(
        string functionName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken ct)
    {
        if (!_registry.TryGet(functionName, out _))
            return new FunctionDispatchResult { Handled = false, Result = null };

        var dict = arguments.ToDictionary(k => k.Key, v => v.Value);
        var res = await _inner.TryDispatchAsync(functionName, dict, ct);

        var ok = (bool)(res?.Success ?? true);
        return new FunctionDispatchResult
        {
            Handled = ok,
            Result = res?.Result
        };
    }
}
