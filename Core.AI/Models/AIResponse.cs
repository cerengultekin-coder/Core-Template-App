namespace Core.AI.Models;

public class AIResponse
{
    public string Content { get; set; } = string.Empty;
    public bool FunctionExecuted { get; set; }
    public string? FunctionName { get; set; }
    public object? FunctionResult { get; set; }
}