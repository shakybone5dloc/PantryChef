using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace PantryChef.Contracts;

[ValidatableType]
public record RegisterRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

[ValidatableType]
public record LoginRequest([Required] string Email, [Required] string Password);

public record AccessToken(string Token, DateTimeOffset ExpiresAt);

public record MeResponse(string Id, string Email);