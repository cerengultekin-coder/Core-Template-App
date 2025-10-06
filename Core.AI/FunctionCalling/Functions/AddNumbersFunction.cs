using System.Text.Json;

namespace Core.AI.FunctionCalling.Functions;

public class AddNumbersFunction : IAiFunction
{
    public string Name => "add_numbers";
    public string Description => "Adds two numbers and returns the sum.";

    public object GetJsonSchema() => new
    {
        type = "object",
        properties = new
        {
            a = new { type = "number", description = "First number" },
            b = new { type = "number", description = "Second number" }
        },
        required = new[] { "a", "b" }
    };

    public async Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, double>>(argumentsJson);

            if (args == null || !args.ContainsKey("a") || !args.ContainsKey("b"))
                return FunctionCallResult.Fail("Invalid arguments. Expected { a: number, b: number }.");

            var sum = args["a"] + args["b"];

            return FunctionCallResult.Ok(new { sum });
        }
        catch (Exception ex)
        {
            return FunctionCallResult.Fail($"Error: {ex.Message}");
        }
    }
}