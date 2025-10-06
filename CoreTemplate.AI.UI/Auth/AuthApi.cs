using CoreApp.Shared.Auth.DTOs;
using System.Net.Http.Json;

namespace CoreTemplate.AI.UI.Auth;

public sealed class AuthApi
{
    private readonly IHttpClientFactory _factory;
    private readonly ITokenStore _store;

    public AuthApi(IHttpClientFactory factory, ITokenStore store)
    {
        _factory = factory;
        _store = store;
    }

    public async Task RegisterAsync(RegisterRequest req)
    {
        var c = _factory.CreateClient("AiApiNoAuth");
        var resp = await c.PostAsJsonAsync("api/auth/register", req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task LoginAsync(LoginRequest req)
    {
        var c = _factory.CreateClient("AiApiNoAuth");
        var resp = await c.PostAsJsonAsync("api/auth/login", req);
        resp.EnsureSuccessStatusCode();

        var dto = await resp.Content.ReadFromJsonAsync<AuthResponse>()
                  ?? throw new InvalidOperationException("Empty auth response.");

        await _store.SetAsync(dto.AccessToken, dto.RefreshToken);
    }
}