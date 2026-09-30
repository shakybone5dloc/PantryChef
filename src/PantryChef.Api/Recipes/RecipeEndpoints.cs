using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using PantryChef.Application.Recipes;
using PantryChef.Contracts;

namespace PantryChef.Api.Recipes;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/recipes/suggest", Suggest).WithTags("Recipes").RequireAuthorization();
        return app;
    }

    private static async Task<Ok<IReadOnlyList<RecipeSuggestion>>> Suggest(
        SuggestRecipesRequest request, IRecipeService recipes, CancellationToken ct) =>
        TypedResults.Ok(await recipes.SuggestAsync(request.Count, request.Preferences, ct));
}