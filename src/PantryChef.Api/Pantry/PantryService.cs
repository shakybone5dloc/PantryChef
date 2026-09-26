namespace PantryChef.Api.Pantry;

public record PantryItem(string Name, decimal Quantity, string Unit);

public interface IPantryService
{
    IReadOnlyList<PantryItem> GetAll();
    PantryItem Add(PantryItem item);
}

public sealed class InMemoryPantryService : IPantryService
{
    private readonly Lock _gate = new();
    private readonly List<PantryItem> _items =
    [
        new("eggs", 12, "count"),
        new("rice", 2, "lb")
    ];

    public IReadOnlyList<PantryItem> GetAll()
    {
        lock (_gate)
        {
            return _items.ToArray();
        }
    }

    public PantryItem Add(PantryItem item)
    {
        lock (_gate)
        {
            _items.Add(item);
        }
        return item;
    }
}