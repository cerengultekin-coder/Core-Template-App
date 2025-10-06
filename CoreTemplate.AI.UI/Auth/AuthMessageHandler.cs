using CoreApp.Shared.Auth;
using CoreApp.Shared.Auth.DTOs;
using CoreTemplate.AI.UI.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _store;
    private readonly IHttpClientFactory _factory;
    private readonly IJSRuntime _js;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public AuthMessageHandler(ITokenStore store, IHttpClientFactory factory, IJSRuntime js)
        => (_store, _factory, _js) = (store, factory, js);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var (access, _) = await _store.GetAsync();
        var clean = TokenHelpers.CleanBearer(access);
        if (!string.IsNullOrWhiteSpace(clean))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

        var resp = await base.SendAsync(request, ct);
        if (resp.StatusCode != HttpStatusCode.Unauthorized) return resp;

        resp.Dispose();

        // refresh
        await Lock.WaitAsync(ct);
        try
        {
            (access, var refresh) = await _store.GetAsync();
            var rClean = TokenHelpers.CleanBearer(refresh);
            if (string.IsNullOrWhiteSpace(rClean))
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var sessionId = await _js.InvokeAsync<string>("ctai.ensureSessionId");
            var client = _factory.CreateClient("AiApiNoAuth");

            var refreshResp = await client.PostAsJsonAsync("api/auth/refresh",
                new { refreshToken = rClean, sessionId }, ct);

            if (!refreshResp.IsSuccessStatusCode)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var dto = await refreshResp.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
            if (dto is null) return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            await _store.SetAsync(TokenHelpers.CleanBearer(dto.AccessToken), dto.RefreshToken);
        }
        finally { Lock.Release(); }

        var clone = await CloneAsync(request);
        var (newAccess, _) = await _store.GetAsync();
        var newClean = TokenHelpers.CleanBearer(newAccess);
        if (!string.IsNullOrWhiteSpace(newClean))
            clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newClean);

        return await base.SendAsync(clone, ct);
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);
        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);
            foreach (var h in req.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        foreach (var h in req.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        clone.Version = req.Version;
        return clone;
    }
}
