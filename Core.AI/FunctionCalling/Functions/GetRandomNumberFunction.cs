using System.Text.Json;

namespace Core.AI.FunctionCalling.Functions;

public class GetRandomNumberFunction : IAiFunction
{
    private readonly Random _random = new();

    public string Name => "get_random_number";
    public string Description => "Returns a random number between min and max (inclusive).";

    public object GetJsonSchema() => new
    {
        type = "object",
        properties = new
        {
            min = new { type = "integer", description = "Minimum value (inclusive)" },
            max = new { type = "integer", description = "Maximum value (inclusive)" }
        },
        required = new[] { "min", "max" }
    };

    public Task<FunctionCallResult> InvokeAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        var args = JsonSerializer.Deserialize<Args>(argumentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (args is null)
            return Task.FromResult(FunctionCallResult.Fail("Invalid arguments."));

        if (args.Min > args.Max)
            return Task.FromResult(FunctionCallResult.Fail("'min' must be <= 'max'."));


        var number = _random.Next(args.Min, args.Max + 1);
        
        return Task.FromResult(FunctionCallResult.Ok(new { value = number }));
    }

    private sealed class Args
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
}