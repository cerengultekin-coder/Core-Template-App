using System.Text.Json.Serialization;

namespace Core.AI.Models.Agent;

public class AgentProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string SystemPrompt { get; set; } = string.Empty;
    public float Temperature { get; set; } = 0.7f;
    [JsonIgnore] 
    public string? Context => SystemPrompt;
}