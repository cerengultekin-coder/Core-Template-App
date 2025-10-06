using Core.AI.Config;
using System.Text.Json.Serialization;

namespace Core.AI.Models;

public class AIRequestOptions
{
    public string? Context { get; set; }
    public string? Purpose { get; set; }
    public string? Language { get; set; }
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
    public string? Model { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AIProvider? Provider { get; set; }
    public bool UseFunctionCalling { get; set; } = true;
    public string? SystemPrompt { get; set; }
}