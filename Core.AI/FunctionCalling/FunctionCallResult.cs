using System.Text.Json;

namespace Core.AI.FunctionCalling;

public class FunctionCallResult
{
    public bool Success { get; private set; }
    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    private FunctionCallResult() { }

    public static FunctionCallResult Ok(object result)
    {
        return new FunctionCallResult
        {
            Success = true,
            Result = JsonSerializer.Serialize(result)
        };
    }

    public static FunctionCallResult Fail(string errorMessage)
    {
        return new FunctionCallResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}