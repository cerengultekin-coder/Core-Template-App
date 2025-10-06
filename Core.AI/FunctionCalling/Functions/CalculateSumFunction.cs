using System.Text.Json;

namespace Core.AI.FunctionCalling.Functions;

public class CalculateSumFunction : IAiFunction
{
    public string Name => "calculate_sum";
    public string Description => "Calculates the sum of two integers.";

    public object GetJsonSchema() => new
    {
        type = "object",
        properties = new
        {
            a = new { type = "integer", description = "First number" },
            b = new { type = "integer", description = "Second number" }
        },
        required = new[] { "a", "b" }
    };

    public Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonSerializer.Deserialize<SumArgs>(argumentsJson);

        if (args == null)
            return Task.FromResult(FunctionCallResult.Fail("Invalid input"));

        return Task.FromResult(FunctionCallResult.Ok(new { sum = args.A + args.B }));
    }

    private class SumArgs
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}