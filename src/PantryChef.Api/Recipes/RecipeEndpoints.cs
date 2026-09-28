using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using PantryChef.Application.Recipes;

namespace PantryChef.Api.Recipes;

public record SuggestRecipesRequest(
    [Range(1, 5)] int Count = 3,
    [StringLength(200)] string? Preferences = null);

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/recipes/suggest", Suggest).WithTags("Recipes");
        return app;
    }

    private static async Task<Ok<IReadOnlyList<RecipeSuggestion>>> Suggest(
        SuggestRecipesRequest request, IRecipeService recipes, CancellationToken ct) =>
        TypedResults.Ok(await recipes.SuggestAsync(request.Count, request.Preferences, ct));
}