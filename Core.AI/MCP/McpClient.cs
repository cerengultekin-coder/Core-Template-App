using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core.AI.MCP;

public sealed class McpClient : IMcpClient, IAsyncDisposable
{
    private readonly ClientWebSocket _ws = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pending = new();
    private long _id;
    private CancellationTokenSource? _listenCts;

    public bool IsConnected => _ws.State == WebSocketState.Open;
    public string? ConnectedServerName { get; private set; }

    public async Task ConnectAsync(McpServerDescriptor server, CancellationToken ct = default)
    {
        if (IsConnected) return;

        if (!server.Transport.Equals("websocket", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Only 'websocket' transport is implemented in this client.");

        var uri = new Uri(server.Endpoint);
        await _ws.ConnectAsync(uri, ct);

        ConnectedServerName = server.Name;

        _listenCts = new CancellationTokenSource();
        _ = Task.Run(() => ListenLoopAsync(_listenCts.Token));
    }

    public async Task<IReadOnlyList<McpToolDescriptor>> ListToolsAsync(CancellationToken ct = default)
    {
        var result = await SendRpcAsync("tools/list", JsonDocument.Parse("{}").RootElement, ct);
        if (result.TryGetProperty("tools", out var tools) && tools.ValueKind == JsonValueKind.Array)
        {
            var list = new System.Collections.Generic.List<McpToolDescriptor>();
            foreach (var t in tools.EnumerateArray())
            {
                list.Add(new McpToolDescriptor
                {
                    Name = t.GetPropertyOrNull("name")?.GetString() ?? "",
                    Description = t.GetPropertyOrNull("description")?.GetString()
                });
            }
            return list;
        }
        return Array.Empty<McpToolDescriptor>();
    }

    public async Task<JsonElement> CallToolAsync(string toolName, JsonElement args, CancellationToken ct = default)
    {
        using var payload = BuildPayload(new { name = toolName, arguments = args });
        var result = await SendRpcAsync("tools/call", payload.RootElement, ct);
        if (result.TryGetProperty("result", out var res))
            return res.Clone();
        return result.Clone();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _listenCts?.Cancel();
            if (_ws.State == WebSocketState.Open)
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "dispose", CancellationToken.None);
        }
        catch { }
        finally
        {
            _ws.Dispose();
            _listenCts?.Dispose();
        }
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        try
        {
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                var sb = new StringBuilder();
                ValueWebSocketReceiveResult res;
                do
                {
                    res = await _ws.ReceiveAsync(buffer.AsMemory(0, buffer.Length), ct);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "server closed", ct);
                        return;
                    }
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, res.Count));
                }
                while (!res.EndOfMessage);

                var text = sb.ToString();
                if (string.IsNullOrWhiteSpace(text)) continue;

                JsonDocument? doc = null;
                try
                {
                    doc = JsonDocument.Parse(text);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                    {
                        var id = idProp.GetInt64();
                        if (_pending.TryRemove(id, out var tcs))
                        {
                            if (root.TryGetProperty("error", out var err))
                                tcs.TrySetException(new InvalidOperationException(err.GetRawText()));
                            else if (root.TryGetProperty("result", out var ok))
                                tcs.TrySetResult(ok.Clone());
                            else
                                tcs.TrySetResult(root.Clone());
                        }
                    }
                }
                catch
                {
                    // TODO: log
                }
                finally
                {
                    doc?.Dispose();
                }
            }
        }
        catch
        {
            // TODO: log + reconnect/backoff
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task<JsonElement> SendRpcAsync(string method, JsonElement @params, CancellationToken ct)
    {
        var id = Interlocked.Increment(ref _id);
        var tcs = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending.TryAdd(id, tcs);

        using var obj = BuildPayload(new { jsonrpc = "2.0", id, method, @params });

        var bytes = Encoding.UTF8.GetBytes(obj.RootElement.GetRawText());
        await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            var result = await tcs.Task.ConfigureAwait(false);
            return result;
        }
    }

    private static JsonDocument BuildPayload(object obj)
        => JsonDocument.Parse(JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
}

internal static class JsonExtensions
{
    public static JsonElement? GetPropertyOrNull(this JsonElement e, string name)
        => e.TryGetProperty(name, out var v) ? v : (JsonElement?)null;
}
