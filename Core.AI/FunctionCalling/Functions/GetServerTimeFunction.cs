namespace Core.AI.FunctionCalling.Functions;
public class GetServerTimeFunction : IAiFunction, IJsonSchemaProvider
{
    public string Name => "get_server_time";
    public string Description => "Returns the current server time in ISO 8601 UTC format.";


    public object GetJsonSchema() => new
    {
        type = "object",
        properties = new { },
        required = Array.Empty<string>()
    };

    public Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.ToString("o");

        return Task.FromResult(FunctionCallResult.Ok(new { time = now }));
    }
}