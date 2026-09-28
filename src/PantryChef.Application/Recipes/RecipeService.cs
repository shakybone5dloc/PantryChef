using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain;

namespace PantryChef.Application.Recipes;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeSuggestion>> SuggestAsync(int count, string? preferences, CancellationToken ct);
}

public sealed class RecipeService(IPantryDbContext db, IRecipeGenerator generator, TimeProvider clock) : IRecipeService
{
    public async Task<IReadOnlyList<RecipeSuggestion>> SuggestAsync(int count, string? preferences, CancellationToken ct)
    {
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

        return await generator.SuggestAsync(pantry, new RecipeRequest(count, preferences), ct);
    }
}