using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain.Pantry;
using PantryChef.Infrastructure.Identity;

namespace PantryChef.Infrastructure.Data;

public class PantryDbContext(DbContextOptions<PantryDbContext> options, ICurrentUser currentUser)
    : IdentityDbContext<AppUser>(options), IPantryDbContext
{
    private string? CurrentUserId => currentUser.UserId;

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<PantryItem> PantryItems => Set<PantryItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Ingredient>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(Ingredient.MaxNameLength);
            e.HasIndex(i => i.Name).IsUnique();
        });

        b.Entity<PantryItem>(e =>
        {
            e.Property(p => p.Unit).HasMaxLength(PantryItem.MaxUnitLength);
            e.Property(p => p.Quantity).HasPrecision(10, 2);
            e.Property(p => p.OwnerId).HasMaxLength(450);

            // A foreign key to AspNetUsers without a navigation property; the Domain stays unaware
            e.HasOne<AppUser>().WithMany().HasForeignKey(p => p.OwnerId).IsRequired();

            e.HasQueryFilter(p => p.OwnerId == CurrentUserId);
        });
    }
}