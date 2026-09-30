using System.ComponentModel.DataAnnotations;

namespace PantryChef.Web.Models;

// What the API returns
public record PantryItem(int Id, string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);
public record AccessToken(string Token, DateTimeOffset ExpiresAt);
public record ProblemDetailsDto(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

// Form models: mutable classes, because forms bind to settable properties
public class LoginForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
}

public class AddItemForm
{
    [Required, StringLength(100)] public string Ingredient { get; set; } = "";
    [Range(0.01, 100000)] public decimal Quantity { get; set; } = 1;
    [Required, StringLength(20)] public string Unit { get; set; } = "count";
    public DateOnly? ExpiresOn { get; set; }
}