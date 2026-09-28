using Microsoft.EntityFrameworkCore;
using PantryChef.Api.Data;
using System.ComponentModel.DataAnnotations;

namespace PantryChef.Api.Pantry;

public record PantryItemResponse(int Id, string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);
public record AddPantryItemRequest(
    [Required, StringLength(100)] string Ingredient, 
    [Range(0.01, 100000)] decimal Quantity, 
    [Required, StringLength(20)] string Unit, 
    DateOnly? ExpiresOn);
public record UpdateQuantityRequest(
    [Range(0, 100000)] decimal Quantity);


public interface IPantryService
{
    Task<IReadOnlyList<PantryItemResponse>> GetAllAsync(CancellationToken ct);
    Task<PantryItemResponse> AddAsync(AddPantryItemRequest request, CancellationToken ct);
    Task<PantryItemResponse> UpdateQuantityAsync(int id, decimal quantity, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public sealed class EfPantryService(PantryDbContext db) : IPantryService
{
    public async Task<IReadOnlyList<PantryItemResponse>> GetAllAsync(CancellationToken ct) =>
        await db.PantryItems
            .OrderBy(p => p.Ingredient.Name)
            .Select(p => new PantryItemResponse(p.Id, p.Ingredient.Name, p.Quantity, p.Unit, p.ExpiresOn))
            .ToListAsync(ct);

    public async Task<PantryItemResponse> AddAsync(AddPantryItemRequest request, CancellationToken ct)
    {
        var name = request.Ingredient.Trim().ToLowerInvariant();

        var ingredient = await db.Ingredients.SingleOrDefaultAsync(i => i.Name == name, ct)
            ?? new Ingredient { Name = name };

        var item = new PantryItem
        {
            Ingredient = ingredient,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ExpiresOn = request.ExpiresOn,
            AddedAt = DateTimeOffset.UtcNow
        };

        db.PantryItems.Add(item);
        await db.SaveChangesAsync(ct);

        return new PantryItemResponse(item.Id, ingredient.Name, item.Quantity, item.Unit, item.ExpiresOn);
    }

    public async Task<PantryItemResponse?> UpdateQuantityAsync(int id, decimal quantity, CancellationToken ct)
    {
        var item = await db.PantryItems
            .Include(p => p.Ingredient)
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (item is null) return null;

        item.Quantity = quantity;
        await db.SaveChangesAsync(ct);

        return new PantryItemResponse(item.Id, item.Ingredient.Name, item.Quantity, item.Unit, item.ExpiresOn);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        // A bulk operation: one DELETE statement, no loading, bypasses the change tracker
        var rows = await db.PantryItems.Where(p => p.Id == id).ExecuteDeleteAsync(ct);
        return rows > 0;
    }
}