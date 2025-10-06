using Microsoft.JSInterop;

namespace CoreTemplate.AI.UI.Auth;

public sealed class SessionStorageTokenStore : ITokenStore
{
    private readonly IJSRuntime _js;
    private const string AccessKey = "auth_access_token";
    private const string RefreshKey = "auth_refresh_token";

    public SessionStorageTokenStore(IJSRuntime js) => _js = js;

    public async Task<(string Access, string Refresh)> GetAsync()
        => (await _js.InvokeAsync<string?>("sess.get", AccessKey) ?? "",
            await _js.InvokeAsync<string?>("sess.get", RefreshKey) ?? "");

    public Task SetAsync(string access, string refresh)
        => Task.WhenAll(
            _js.InvokeVoidAsync("sess.set", AccessKey, access).AsTask(),
            _js.InvokeVoidAsync("sess.set", RefreshKey, refresh).AsTask());

    public Task ClearAsync()
        => Task.WhenAll(
            _js.InvokeVoidAsync("sess.remove", AccessKey).AsTask(),
            _js.InvokeVoidAsync("sess.remove", RefreshKey).AsTask());
}