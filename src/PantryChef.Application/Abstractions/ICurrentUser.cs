namespace PantryChef.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
}