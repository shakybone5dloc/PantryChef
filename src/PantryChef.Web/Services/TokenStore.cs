using Microsoft.JSInterop;

namespace PantryChef.Web.Services;

// Keeps the JWT in the browser's localStorage, so the login survives a page refresh
public sealed class TokenStore(IJSRuntime js)
{
    private const string Key = "pantrychef.token";
    private string? _token;
    private bool _loaded;

    public async ValueTask<string?> GetAsync()
    {
        if (!_loaded)
        {
            _token = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            _loaded = true;
        }
        return _token;
    }

    public async ValueTask SetAsync(string token)
    {
        _token = token;
        _loaded = true;
        await js.InvokeVoidAsync("localStorage.setItem", Key, token);
    }

    public async ValueTask ClearAsync()
    {
        _token = null;
        _loaded = true;
        await js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}