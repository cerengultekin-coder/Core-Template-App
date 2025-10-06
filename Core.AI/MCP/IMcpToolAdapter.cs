using System.Text.Json;

namespace Core.AI.MCP;

public interface IMcpToolAdapter
{
    bool IsEnabled { get; }
    Task<JsonElement> CallAsync(string toolName, JsonElement args, CancellationToken ct);
}
