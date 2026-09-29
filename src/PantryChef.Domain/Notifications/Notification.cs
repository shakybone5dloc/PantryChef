namespace PantryChef.Domain.Notifications;

public class Notification
{
    public const int MaxMessageLength = 2000;

    public int Id { get; private set; }
    public string OwnerId { get; private set; } = null!;
    public DateOnly ForDate { get; private set; }
    public string Message { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    public Notification(string ownerId, DateOnly forDate, string message, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        OwnerId = ownerId;
        ForDate = forDate;
        Message = message.Length <= MaxMessageLength ? message : message[..(MaxMessageLength - 1)] + "...";
        CreatedAt = createdAt;
    }

    public void MarkRead(DateTimeOffset at) => ReadAt ??= at;
}