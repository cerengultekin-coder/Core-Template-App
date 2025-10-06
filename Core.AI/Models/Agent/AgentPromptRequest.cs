namespace Core.AI.Models.Agent;

public class AgentPromptRequest
{
    public string Prompt { get; set; } = "";
    public AgentRequestOptions? Options { get; set; }
    public string? UserId { get; set; }
}