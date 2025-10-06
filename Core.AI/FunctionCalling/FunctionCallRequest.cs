namespace Core.AI.FunctionCalling;

public class FunctionCallRequest
{
    public string Prompt { get; set; } = string.Empty;
    public FunctionCallOptions? Options { get; set; }
}
