using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace PantryChef.Web.Services;

// Tells Blazor who the current user is, based on the stored JWT
public sealed class JwtAuthStateProvider(TokenStore tokens) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokens.GetAsync();
        if (string.IsNullOrEmpty(token)) return Anonymous;

        var claims = ReadClaims(token);
        var exp = claims.FirstOrDefault(claims => claims.Type == "exp")?.Value;
        if (exp is null || DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) <= DateTimeOffset.UtcNow)
        {
            await tokens.ClearAsync();   // expired: treat as signed out
            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: "email", roleType: "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string token)
    {
        await tokens.SetAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await tokens.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    // READS the payload but doesn't VERIFY the signature. The client only needs it for display;
    // verification is the API's job, and it happens on every request.
    private static List<Claim> ReadClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
        return doc.RootElement.EnumerateObject().Select(p => new Claim(p.Name, p.Value.ToString())).ToList();
    }
}