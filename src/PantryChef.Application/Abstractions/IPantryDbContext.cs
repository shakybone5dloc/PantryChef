using Microsoft.EntityFrameworkCore;
using PantryChef.Domain.Pantry;
using PantryChef.Domain.Notifications;

namespace PantryChef.Application.Abstractions;

public interface IPantryDbContext
{
    DbSet<Ingredient> Ingredients { get; }
    DbSet<PantryItem> PantryItems { get; }
    DbSet<Notification> Notifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}