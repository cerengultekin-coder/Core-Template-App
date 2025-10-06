using System.Collections.Generic;

namespace Core.AI.MCP;

public sealed class McpOptions
{
    public string? DefaultServer { get; set; }
    public List<McpServerDescriptor> Servers { get; set; } = new();
}
