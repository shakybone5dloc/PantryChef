using System.ComponentModel.DataAnnotations;

namespace PantryChef.Api.Recipes;

public sealed class RecipeOptions
{
    public const string SectionName = "Recipes";

    [Required]
    public string Model { get; set; } = "";

    [Range(1, 10)]
    public int MaxSuggestions { get; set; } = 3;

    [Required]
    public string ApiKey { get; set; } = "";
}