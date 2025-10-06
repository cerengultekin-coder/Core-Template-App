using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace CoreTemplate.AI.UI.Auth;

public sealed class AuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private const string AccessKey = "auth_access_token";
    private const string RefreshKey = "auth_refresh_token";

    public AuthStateProvider(IJSRuntime js) => _js = js;

    public async Task SignInAsync(string access, string refresh)
    {
        await _js.InvokeVoidAsync("sess.set", AccessKey, access);
        await _js.InvokeVoidAsync("sess.set", RefreshKey, refresh);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _js.InvokeVoidAsync("sess.remove", AccessKey);
        await _js.InvokeVoidAsync("sess.remove", RefreshKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Task<string?> GetAccessTokenAsync()
        => _js.InvokeAsync<string?>("sess.get", AccessKey).AsTask();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token) || IsExpired(token))
            return new(new ClaimsPrincipal(new ClaimsIdentity()));

        var identity = new ClaimsIdentity(ParseClaims(token), "jwt");
        return new(new ClaimsPrincipal(identity));
    }

    private static bool IsExpired(string jwt)
    {
        var payload = GetPayload(jwt);
        if (payload is null) return true;

        if (payload.Value.TryGetProperty("nbf", out var nbf))
        {
            var nbfUtc = DateTimeOffset.FromUnixTimeSeconds(nbf.GetInt64()).UtcDateTime;
            if (nbfUtc > DateTime.UtcNow) return true;
        }

        if (!payload.Value.TryGetProperty("exp", out var exp)) return true;
        var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()).UtcDateTime;
        return expUtc <= DateTime.UtcNow;
    }


    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = GetPayload(jwt);
        if (payload is null) yield break;

        foreach (var kv in payload.Value.EnumerateObject())
        {
            if (kv.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in kv.Value.EnumerateArray())
                    yield return new Claim(kv.Name, v.ToString());
            }
            else yield return new Claim(kv.Name, kv.Value.ToString());
        }
    }

    private static JsonElement? GetPayload(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            var p = parts[1].PadRight((int)Math.Ceiling(parts[1].Length / 4m) * 4, '=')
                           .Replace('-', '+').Replace('_', '/');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(p));
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch { return null; }
    }
}
