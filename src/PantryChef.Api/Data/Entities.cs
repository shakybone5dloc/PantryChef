namespace PantryChef.Api.Data;

public class Ingredient
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<PantryItem> PantryItems { get; set; } = [];
}

public class PantryItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal Quantity { get; set; }
    public required string Unit {  get; set; }
    public DateOnly? ExpiresOn { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}