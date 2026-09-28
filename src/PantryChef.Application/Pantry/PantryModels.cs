namespace PantryChef.Application.Pantry;

public record PantryItemDto(int Id, string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);

public record AddPantryItem(string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);