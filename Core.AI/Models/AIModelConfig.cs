using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AI.Models;

public class AIModelConfig
{
    public required AIModel Model { get; set; }

    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 1.0;
    public int MaxTokens { get; set; } = 1000;

    public string? SystemPrompt { get; set; }
    public bool UseFunctionCalling { get; set; } = false;

    public string? AgentProfileId { get; set; }
}
