using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain.Notifications;

namespace PantryChef.Application.Notifications;

public interface IExpiryDigestService
{
    /// <returns>How many notifications were created.</returns>
    Task<int> RunAsync(int daysAhead, CancellationToken ct);
}

public sealed class ExpiryDigestService(IPantryDbContext db, TimeProvider clock) : IExpiryDigestService
{
    public async Task<int> RunAsync(int daysAhead, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var until = today.AddDays(daysAhead);

        // SYSTEM JOB: no user is logged in, so the per-user query filter would hide everything.
        // We bypass it deliberately, and every step below must keep users separated view OwnerId.
        var expiring = await db.PantryItems.IgnoreQueryFilters()
            .Where(p => p.Quantity > 0 && p.ExpiresOn != null && p.ExpiresOn >= today && p.ExpiresOn <= until)
            .Select(p => new { p.OwnerId, p.Ingredient.Name, ExpiresOn = p.ExpiresOn!.Value })
            .ToListAsync(ct);

        var alreadyNotified = (await db.Notifications.IgnoreQueryFilters()
            .Where(n => n.ForDate == today)
            .Select(n => n.OwnerId)
            .ToListAsync(ct)).ToHashSet();

        var created = 0;
        foreach (var owner in expiring.GroupBy(x => x.OwnerId).Where(g => !alreadyNotified.Contains(g.Key)))
        {
            var items = string.Join(", ", owner
                .OrderBy(x => x.ExpiresOn)
                .Select(x => $"{x.Name} ({x.ExpiresOn:MMM d})"));

            db.Notifications.Add(new Notification(owner.Key, today, $"Use soon: {items}", now));
            created++;
        }

        await db.SaveChangesAsync(ct);
        PantryChefTelemetry.DigestNotifications.Add(created);
        return created;
    }
}