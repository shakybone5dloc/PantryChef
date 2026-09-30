namespace PantryChef.Contracts;

public record NotificationDto(int Id, DateOnly ForDate, string Message, DateTimeOffset CreatedAt, bool IsRead);