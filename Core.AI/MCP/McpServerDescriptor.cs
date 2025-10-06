namespace Core.AI.MCP;

public sealed class McpServerDescriptor
{
    public string Name { get; set; } = "";
    public string Transport { get; set; } = "websocket";
    public string Endpoint { get; set; } = "";
    public string? ApiKey { get; set; }
}
