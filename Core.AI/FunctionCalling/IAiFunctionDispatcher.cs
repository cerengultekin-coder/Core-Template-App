using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.FunctionCalling;

public interface IAiFunctionDispatcher
{
    Task<FunctionDispatchResult> TryDispatchAsync(
        string functionName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken ct);
}
