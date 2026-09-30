using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain;
using PantryChef.Contracts;

namespace PantryChef.Application.Recipes;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeSuggestion>> SuggestAsync(int count, string? preferences, CancellationToken ct);
}

public sealed class RecipeService(IPantryDbContext db, IRecipeGenerator generator, TimeProvider clock) : IRecipeService
{
    public async Task<IReadOnlyList<RecipeSuggestion>> SuggestAsync(int count, string? preferences, CancellationToken ct)
    {
        using var activity = PantryChefTelemetry.ActivitySource.StartActivity("SuggestRecipes");
        activity?.SetTag("pantrychef.recipes.requested", count);

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var soon = today.AddDays(3);

        var pantry = await db.PantryItems
            .Where(p => p.Quantity > 0)
            .Where(p => p.ExpiresOn == null || p.ExpiresOn >= today)
            .OrderBy(p => p.ExpiresOn ?? DateOnly.MaxValue)
            .Select(p => new PantryIngredient(
                p.Ingredient.Name, p.Quantity, p.Unit,
                p.ExpiresOn != null && p.ExpiresOn <= soon))
            .ToListAsync(ct);

        if (pantry.Count == 0)
            throw new DomainException("Your pantry is empty. Add some ingredients first.");

        activity?.SetTag("pantrychef.pantry.size", pantry.Count);

        var start = clock.GetTimestamp();
        try
        {
            var recipes = await generator.SuggestAsync(pantry, new RecipeRequest(count, preferences), ct);
            PantryChefTelemetry.RecipeRequests.Add(1, new KeyValuePair<string, object?>("outcome", "success"));
            activity?.SetTag("pantrychef.recipes.returned", recipes.Count);
            return recipes;
        }
        catch (Exception ex)
        {
            PantryChefTelemetry.RecipeRequests.Add(1, new KeyValuePair<string, object?>("outcome", "failure"));
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            PantryChefTelemetry.RecipeGenerationDuration.Record(clock.GetElapsedTime(start).TotalSeconds);
        }
    }
}