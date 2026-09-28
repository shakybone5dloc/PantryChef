namespace PantryChef.Domain.Pantry;

public class Ingredient
{
    public const int MaxNameLength = 100;

    private readonly List<PantryItem> _pantryItems = [];

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public IReadOnlyCollection<PantryItem> PantryItems => _pantryItems;

    private Ingredient() { }

    public Ingredient(string name) => Name = Normalize(name);

    public static string Normalize(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim().ToLowerInvariant();
        if (normalized.Length > MaxNameLength)
            throw new DomainException($"Ingredient name cannot exceed {MaxNameLength} characters.");
        return normalized;
    }
}