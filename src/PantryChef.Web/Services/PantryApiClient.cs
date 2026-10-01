using System.Diagnostics;
using System.Net.Http.Json;
using PantryChef.Web.Models;
using PantryChef.Contracts;

namespace PantryChef.Web.Services;

// A typed client: the rest of the app calls methods, not URLs
public sealed class PantryApiClient(HttpClient http)
{
    public async Task<AccessToken?> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("/auth/login", new LoginRequest(email, password));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<AccessToken>() : null;
    }

    public async Task<string?> RegisterAsync(string email, string password) =>
        await ErrorOrNull(await http.PostAsJsonAsync("/auth/register", new RegisterRequest(email, password)));

    public async Task<List<PantryItemDto>> GetPantryAsync() =>
        await http.GetFromJsonAsync<List<PantryItemDto>>("/pantry") ?? [];

    public async Task<string?> AddAsync(AddItemForm f) =>
        await ErrorOrNull(await http.PostAsJsonAsync("/pantry",
            new AddPantryItemRequest(f.Ingredient, f.Quantity, f.Unit, f.ExpiresOn)));

    public async Task<string?> UseAsync(int id, decimal amount) =>
        await ErrorOrNull(await http.PostAsJsonAsync($"/pantry/{id}/use", new UseItemRequest(amount)));
    
    public async Task<string?> DeleteAsync(int id) =>
        await ErrorOrNull(await http.DeleteAsync($"/pantry/{id}"));

    public async Task<List<RecipeSuggestion>> SuggestRecipesAsync(SuggestRecipesRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/recipes/suggest", request, ct);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<RecipeSuggestion>>(ct) ?? [];

        throw new ApiException(await ErrorOrNull(response) ?? "Request failed.", (int)response.StatusCode);
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync() =>
        await http.GetFromJsonAsync<List<NotificationDto>>("/notifications") ?? [];

    public async Task MarkNotificationReadAsync(int id) =>
        (await http.PostAsync($"/notifications/{id}/read", null)).EnsureSuccessStatusCode();

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