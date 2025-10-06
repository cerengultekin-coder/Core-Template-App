using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.AI.FunctionCalling;

namespace Core.AI.MCP;

public sealed class McpFunctionDispatcherProxy : IAiFunctionDispatcher
{
    private readonly IAiFunctionDispatcher _inner;
    private readonly IMcpToolAdapter _mcp;

    public McpFunctionDispatcherProxy(IAiFunctionDispatcher inner, IMcpToolAdapter mcp)
    {
        _inner = inner;
        _mcp = mcp;
    }

    public async Task<FunctionDispatchResult> TryDispatchAsync(
        string functionName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken ct)
    {
        var local = await _inner.TryDispatchAsync(functionName, arguments, ct);
        if (local.Handled) return local;

        if (!_mcp.IsEnabled)
            return new FunctionDispatchResult { Handled = false, Result = null };

        var args = JsonSerializer.SerializeToElement(arguments);
        var res = await _mcp.CallAsync(functionName, args, ct);
        return new FunctionDispatchResult
        {
            Handled = true,
            Result = res.GetRawText()
        };
    }
}
