using Microsoft.Extensions.Logging.Abstractions;
using OllamaSharp;
using PantryChef.Application.Recipes;
using PantryChef.Infrastructure.AI;
using PantryChef.Tests.Fakes;
using Xunit;

namespace PantryChef.Tests.Unit.AI;

public class AiRecipeGeneratorTests
{
    private readonly FakeChatClient _chat = new();
    private readonly AiRecipeGenerator _sut;

    private static readonly IReadOnlyList<PantryIngredient> Pantry =
        [
            new("spinach", 1, "bag", ExpiresSoon: true),
            new("rice", 2, "lb", ExpiresSoon: false)
        ];

    public AiRecipeGeneratorTests() =>
        _sut = new AiRecipeGenerator(_chat, NullLogger<AiRecipeGenerator>.Instance);

    private Task<IReadOnlyList<RecipeSuggestion>> Suggest(int count = 2) =>
        _sut.SuggestAsync(Pantry, new RecipeRequest(count, null), TestContext.Current.CancellationToken);

    [Fact]
    public async Task ValidJson_IsMappedToSuggestions()
    {
        var recipe = Assert.Single(await Suggest());

        Assert.Equal("Spinach Rice", recipe.Title);
        Assert.Equal(2, recipe.Ingredients.Count);
        Assert.Equal("1 cup", recipe.Ingredients[1].Amount);
    }

    [Fact]
    public async Task MoreRecipesThanRequested_AreTrimmed()
    {
        _chat.ResponseText = """
            { "recipes": [
                { "title": "A", "summary": "a", "totalMinutes": 1 },
                { "title": "B", "summary": "b", "totalMinutes": 1 },
                { "title": "C", "summary": "c", "totalMinutes": 1 } ] }
            """;

        Assert.Equal(2, (await Suggest(count: 2)).Count);
    }

    [Theory]
    [InlineData("Sure! Here are some recipes you might enjoy...")]
    [InlineData("""{ "recipes": [] }""")]
    public async Task UnusableOutput_ThrowsRecipeGenerationException(string output)
    {
        _chat.ResponseText = output;

        await Assert.ThrowsAsync<RecipeGenerationException>(() => Suggest());
    }

    [Fact]
    public async Task ProviderFailure_IsWrapped_WithOriginalAsInner()
    {
        _chat.ExceptionToThrow = new HttpRequestException("connection refused");

        var ex = await Assert.ThrowsAsync<RecipeGenerationException>(() => Suggest());

        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task Cancellation_IsNotWrapped()
    {
        _chat.ExceptionToThrow = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => Suggest());
    }

    [Fact]
    public async Task Prompt_FlagsOnlyExpiringIngredients()
    {
        await Suggest();

        var prompt = _chat.LastMessages[^1].Text;
        Assert.Contains("- spinach: 1 bag (expires soon)", prompt);
        Assert.Contains("- rice: 2 lb", prompt);
        Assert.DoesNotContain("rice: 2 lb (expires soon)", prompt);
    }
}