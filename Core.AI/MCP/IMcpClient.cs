using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.AI.MCP;

public interface IMcpClient
{
    Task ConnectAsync(McpServerDescriptor server, CancellationToken ct = default);
    Task<IReadOnlyList<McpToolDescriptor>> ListToolsAsync(CancellationToken ct = default);
    Task<JsonElement> CallToolAsync(string toolName, JsonElement args, CancellationToken ct = default);

    bool IsConnected { get; }
    string? ConnectedServerName { get; }
}
