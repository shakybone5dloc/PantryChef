using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;

namespace PantryChef.Application.Notifications;

public record NotificationDto(int Id, DateOnly ForDate, string Message, DateTimeOffset CreatedAt, bool IsRead);

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(CancellationToken ct);
    Task<bool> MarkReadAsync(int i, CancellationToken ct);
}

// Run inside a request: the global query filter scopes everything to the caller
public sealed class NotificationService(IPantryDbContext db, TimeProvider clock) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(CancellationToken ct) =>
        await db.Notifications
            .OrderBy(n => n.ReadAt != null)
            .ThenByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.ForDate, n.Message, n.CreatedAt, n.ReadAt != null))
            .ToListAsync(ct);

    public async Task<bool> MarkReadAsync(int id, CancellationToken ct)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(n => n.Id == id, ct);
        if (notification is null) return false;

        notification.MarkRead(clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return true;
    }
}