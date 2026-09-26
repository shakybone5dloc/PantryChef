using Microsoft.EntityFrameworkCore;

namespace PantryChef.Api.Data;

public class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ingredient>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(100);
            e.HasIndex(i => i.Name).IsUnique();
        });

        b.Entity<PantryItem>(e =>
        {
            e.Property(p => p.Unit).HasMaxLength(20);
            e.Property(p => p.Quantity).HasPrecision(10, 2);
        });
    }
}