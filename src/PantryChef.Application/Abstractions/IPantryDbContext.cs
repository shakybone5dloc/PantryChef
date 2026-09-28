using Microsoft.EntityFrameworkCore;
using PantryChef.Domain.Pantry;

namespace PantryChef.Application.Abstractions;

public interface IPantryDbContext
{
    DbSet<Ingredient> Ingredients { get; }
    DbSet<PantryItem> PantryItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}