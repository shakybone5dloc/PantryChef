namespace PantryChef.Application.Pantry;

public record AddPantryItem(string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);