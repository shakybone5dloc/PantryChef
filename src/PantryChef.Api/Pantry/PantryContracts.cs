using System.ComponentModel.DataAnnotations;

namespace PantryChef.Api.Pantry;

public record AddPantryItemRequest(
    [Required, StringLength(100)] string Ingredient,
    [Range(0.01, 100000)] decimal Quantity,
    [Required, StringLength(20)] string Unit,
    DateOnly? ExpiresOn);

public record UpdateQuantityRequest([Range(0, 100000)] decimal Quantity);

public record UseItemRequest([Range(0.01, 100000)] decimal Amount);