namespace PantryChef.Domain.Pantry;

public class PantryItem
{
    public const int MaxUnitLength = 20;

    public int Id { get; private set; }
    public int IngredientId { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = null!;
    public DateOnly? ExpiresOn { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
    public string OwnerId { get; private set; } = null!;

    private PantryItem() { }

    public PantryItem(string ownerId, Ingredient ingredient, decimal quantity, string unit, DateOnly? expiresOn, DateTimeOffset addedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentNullException.ThrowIfNull(ingredient);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        OwnerId = ownerId;
        Ingredient = ingredient;
        SetQuantity(quantity);
        Unit = unit.Trim().ToLowerInvariant();
        ExpiresOn = expiresOn;
        AddedAt = addedAt;
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity < 0) throw new DomainException("Quantity cannot be negative.");
        Quantity = quantity;
    }

    public void Consume(decimal amount)
    {
        if (amount <= 0) throw new DomainException("Amount used must be positive.");
        if (amount > Quantity) throw new DomainException($"Only {Quantity} {Unit} left.");
        Quantity -= amount;
    }

    public bool IsExpired(DateOnly today) => ExpiresOn is { } d && d < today;
}