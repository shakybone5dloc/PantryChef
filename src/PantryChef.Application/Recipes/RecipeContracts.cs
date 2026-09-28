namespace PantryChef.Application.Recipes;

public record PantryIngredient(string Name, decimal Quantity, string Unit, bool ExpiresSoon);

public record RecipeRequest(int Count, string? Preferences);

public record RecipeIngredient(string Name, string Amount);

public record RecipeSuggestion(
    string Title,
    string Summary,
    int TotalMinutes,
    IReadOnlyList<RecipeIngredient> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> MissingIngredients);

// The PORT: Application say WHAT it needs; Infrastructure decides HOW.
public interface IRecipeGenerator
{
    Task<IReadOnlyList<RecipeSuggestion>> SuggestAsync(
        IReadOnlyList<PantryIngredient> patry, RecipeRequest request, CancellationToken ct);
}

// Raised by any generator when the provider fails. The Api maps it to 503.
public class RecipeGenerationException(string message, Exception? inner = null) : Exception(message, inner);