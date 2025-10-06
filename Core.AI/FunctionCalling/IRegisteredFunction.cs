namespace Core.AI.FunctionCalling;

public interface IRegisteredFunction
{
    Task<string?> InvokeAsync(IReadOnlyDictionary<string, object?> args, CancellationToken ct);
}
