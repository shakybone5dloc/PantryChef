using System.Net;
using System.Net.Http.Headers;

namespace PantryChef.Web.Services;

// Middleware for OUTGOING requests: adds the token, and signs out of the API rejects it
public sealed class AuthHeaderHandler(TokenStore tokens, JwtAuthStateProvider auth) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await tokens.GetAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        // The API is the source of truth. If it says 401 (expired token, delete user), drop the token.
        if (response.StatusCode == HttpStatusCode.Unauthorized && token is not null)
            await auth.SignOutAsync();

        return response;
    }
}