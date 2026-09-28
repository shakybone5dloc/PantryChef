using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain.Pantry;

namespace PantryChef.Infrastructure.Data;

public class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options), IPantryDbContext
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ingredient>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(Ingredient.MaxNameLength);
            e.HasIndex(i => i.Name).IsUnique();
        });

        b.Entity<PantryItem>(e =>
        {
            e.Property(p => p.Unit).HasMaxLength(PantryItem.MaxUnitLength);
            e.Property(p => p.Quantity).HasPrecision(10, 2);
        });
    }
}