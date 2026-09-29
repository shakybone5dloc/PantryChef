using PantryChef.Domain;
using PantryChef.Domain.Pantry;
using Xunit;

namespace PantryChef.Tests.Unit.Domain;

public class IngredientTests
{
    [Theory]
    [InlineData("Eggs", "eggs")]
    [InlineData("  Green Onion  ", "green onion")]
    public void Normalize_TrimsAndLowercases(string input, string expected) =>
        Assert.Equal(expected, Ingredient.Normalize(input));

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public void Normalize_Blank_Throws(string input) =>
        Assert.Throws<ArgumentException>(() => Ingredient.Normalize(input));

    [Fact]
    public void Normalize_TooLong_Throws() =>
        Assert.Throws<DomainException>(() => Ingredient.Normalize(new string('a', Ingredient.MaxNameLength + 1)));
}