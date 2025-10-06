namespace Core.AI.FunctionCalling;

public interface IAiFunction
{
    string Name { get; }
    string Description { get; }
    Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken);
}