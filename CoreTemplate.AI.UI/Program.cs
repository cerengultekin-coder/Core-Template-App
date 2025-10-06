using CoreTemplate.AI.UI;
using CoreTemplate.AI.UI.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7059/";

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());

builder.Services.AddScoped<ITokenStore, SessionStorageTokenStore>();
builder.Services.AddTransient<AuthMessageHandler>();

builder.Services.AddScoped<AuthApi>();

builder.Services.AddHttpClient("AiApi", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddHttpClient("AiApiNoAuth", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

await builder.Build().RunAsync();
