using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace PantryChef.Contracts;

[ValidatableType]
public record SuggestRecipesRequest(
    [Range(1, 5)] int Count = 3,
    [StringLength(200)] string? Preferences = null);

public record RecipeIngredient(string Name, string Amount);

public record RecipeSuggestion(
    string Title,
    string Summary,
    int TotalMinutes,
    IReadOnlyList<RecipeIngredient> Ingredients,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> MissingIngredients);