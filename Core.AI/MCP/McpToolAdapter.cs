using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Core.AI.MCP;

public sealed class McpToolAdapter : IMcpToolAdapter
{
    private readonly IMcpClient _client;
    private readonly McpOptions _options;

    public McpToolAdapter(IMcpClient client, IOptions<McpOptions> opt)
    {
        _client = client;
        _options = opt.Value;
    }

    public bool IsEnabled => _options.Servers?.Count > 0;

    public async Task<JsonElement> CallAsync(string toolName, JsonElement args, CancellationToken ct)
    {
        if (!IsEnabled) throw new System.InvalidOperationException("MCP not configured.");

        if (!_client.IsConnected)
        {
            var server = ResolveServer();
            await _client.ConnectAsync(server, ct);
        }
        return await _client.CallToolAsync(toolName, args, ct);
    }

    private McpServerDescriptor ResolveServer()
    {
        if (!string.IsNullOrWhiteSpace(_options.DefaultServer))
        {
            var found = _options.Servers.Find(s =>
                s.Name.Equals(_options.DefaultServer, System.StringComparison.OrdinalIgnoreCase));
            if (found is not null) return found;
        }
        if (_options.Servers.Count == 0)
            throw new System.InvalidOperationException("No MCP servers configured.");
        return _options.Servers[0];
    }
}
