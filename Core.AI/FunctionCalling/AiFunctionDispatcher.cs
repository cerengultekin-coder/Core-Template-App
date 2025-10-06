using System.Text.Json;

namespace Core.AI.FunctionCalling;

public class AiFunctionDispatcher
{
    private readonly Dictionary<string, IAiFunction> _functions;

    public AiFunctionDispatcher(IEnumerable<IAiFunction> functions) => _functions = functions.ToDictionary(f => f.Name);

    public async Task<FunctionCallResult> TryDispatchAsync(string functionName, Dictionary<string, object> args, CancellationToken cancellationToken)
    {
        if (!_functions.TryGetValue(functionName, out var function))
            return FunctionCallResult.Fail($"Function '{functionName}' not found.");


        var jsonArgs = JsonSerializer.Serialize(args);

        return await function.InvokeAsync(jsonArgs, cancellationToken);
    }
}
