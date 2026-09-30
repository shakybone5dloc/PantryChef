using System.Diagnostics;
using System.Net.Http.Json;
using PantryChef.Web.Models;

namespace PantryChef.Web.Services;

// A typed client: the rest of the app calls methods, not URLs
public sealed class PantryApiClient(HttpClient http)
{ 
    public async Task<AccessToken?> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("/auth/login", new { email, password });
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<AccessToken>() : null;
    }

    public async Task<string?> RegisterAsync(string email, string password) =>
        await ErrorOrNull(await http.PostAsJsonAsync("/auth/register", new { email, password }));

    public async Task<List<PantryItem>> GetPantryAsync() =>
        await http.GetFromJsonAsync<List<PantryItem>>("/pantry") ?? [];

    public async Task<string?> AddAsync(AddItemForm f) =>
        await ErrorOrNull(await http.PostAsJsonAsync("/pantry", new { f.Ingredient, f.Quantity, f.Unit, f.ExpiresOn }));

    public async Task<string?> UseAsync(int id, decimal amount) =>
        await ErrorOrNull(await http.PostAsJsonAsync($"/pantry/{id}/use", new { amount }));

    public async Task<string?> DeleteAsync(int id) =>
        await ErrorOrNull(await http.DeleteAsync($"/pantry/{id}"));

    // Turns a ProblemDetails error response (Module 3) into one readable message, or null on success
    private static async Task<string?> ErrorOrNull(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return null;

        ProblemDetailsDto? problem = null;
        try { problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(); } catch { /* no JSON body */ }

        return problem?.Errors?.SelectMany(e => e.Value).FirstOrDefault()
            ?? problem?.Detail
            ?? problem?.Title
            ?? $"Request failed ({(int)response.StatusCode}).";
    }
}