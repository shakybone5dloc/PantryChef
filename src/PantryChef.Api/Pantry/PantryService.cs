using Microsoft.EntityFrameworkCore;
using PantryChef.Api.Data;

namespace PantryChef.Api.Pantry;

public record PantryItemResponse(int Id, string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);
public record AddPantryItemRequest(string Ingredient, decimal Quantity, string Unit, DateOnly? ExpiresOn);

public interface IPantryService
{
    Task<IReadOnlyList<PantryItemResponse>> GetAllAsync(CancellationToken ct);
    Task<PantryItemResponse> AddAsync(AddPantryItemRequest request, CancellationToken ct);
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
}