using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace PantryChef.Contracts;

public record PantryItemDto(int Id, string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);

[ValidatableType]
public record AddPantryItemRequest(
    [Required, StringLength(100)] string Ingredient,
    [Range(0.01, 100000)] decimal Quantity,
    [Required, StringLength(20)] string Unit,
    DateOnly? ExpiresOn);

[ValidatableType]
public record UpdateQuantityRequest([Range(0, 100000)] decimal Quantity);

[ValidatableType]
public record UseItemRequest([Range(0.01, 100000)] decimal Amount);