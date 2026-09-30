using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PantryChef.Web;
using PantryChef.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddSingleton<JwtAuthStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient<PantryApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBase);
    c.Timeout = TimeSpan.FromMinutes(3);
})
    .AddHttpMessageHandler<AuthHeaderHandler>();

await builder.Build().RunAsync();