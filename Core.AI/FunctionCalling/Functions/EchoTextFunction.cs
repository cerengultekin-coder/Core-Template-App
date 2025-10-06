using System.Text.Json;

namespace Core.AI.FunctionCalling.Functions;

public class EchoTextFunction : IAiFunction
{
    public string Name => "echo_text";
    public string Description => "Returns the text as is.";

    public object GetJsonSchema() => new
    {
        type = "object",
        properties = new
        {
            text = new { type = "string", description = "The text to return as-is" }
        },
        required = new[] { "text" }
    };

    public Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);

        if (args == null || !args.ContainsKey("text"))
            return Task.FromResult(FunctionCallResult.Fail("Missing 'text' field"));

        return Task.FromResult(FunctionCallResult.Ok(new { result = args["text"] }));
    }
}
