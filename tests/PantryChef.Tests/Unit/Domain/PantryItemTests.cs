using PantryChef.Domain;
using PantryChef.Domain.Pantry;
using Xunit;

namespace PantryChef.Tests.Unit.Domain;

public class PantryItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private static PantryItem NewItem(decimal quantity = 12, DateOnly? expiresOn = null) =>
        new(new Ingredient("eggs"), quantity, "count", expiresOn, Now);

    [Fact]
    public void Consume_ReducesQuantity()
    {
        var item = NewItem(quantity: 12);

        item.Consume(4);

        Assert.Equal(8, item.Quantity);
    }

    [Fact]
    public void Consume_MoreThanAvailable_Throws_AndLeavesQuantityUnchanged()
    {
        var item = NewItem(quantity: 3);

        var ex = Assert.Throws<DomainException>(() => item.Consume(5));

        Assert.Contains("Only 3", ex.Message);
        Assert.Equal(3, item.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Consume_NonPositiveAmount_Throws(double amount)
    {
        var item = NewItem();
        Assert.Throws<DomainException>(() => item.Consume((decimal)amount));
    }

    [Fact]
    public void Constructor_NegativeQuantity_Throws() =>
        Assert.Throws<DomainException>(() => NewItem(quantity: -1));

    [Theory]
    [InlineData("2026-09-30", true)]
    [InlineData("2026-10-01", false)]
    [InlineData("2026-10-05", false)]
    public void IsExpired_ComparesAgainstToday(string? expiresOn, bool expected)
    {
        var item = NewItem(expiresOn: expiresOn is null ? null : DateOnly.Parse(expiresOn));

        Assert.Equal(expected, item.IsExpired(new DateOnly(2026, 10, 1)));
    }
}