using Microsoft.EntityFrameworkCore;
using PantryChef.Application.Abstractions;
using PantryChef.Domain.Pantry;

namespace PantryChef.Application.Pantry;

public interface IPantryService
{
    Task<IReadOnlyList<PantryItemDto>> GetAllAsync(CancellationToken ct);
    Task<PantryItemDto> AddAsync(AddPantryItem command,  CancellationToken ct);
    Task<PantryItemDto?> SetQuantityAsync(int id, decimal quantity, CancellationToken ct);
    Task<PantryItemDto?> UseAsync(int id, decimal amount, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public sealed class PantryService(IPantryDbContext db, TimeProvider clock) : IPantryService
{
    public async Task<IReadOnlyList<PantryItemDto>> GetAllAsync(CancellationToken ct) =>
        await db.PantryItems
            .OrderBy(p => p.Ingredient.Name)
            .Select(p => new PantryItemDto(p.Id, p.Ingredient.Name, p.Quantity, p.Unit, p.ExpiresOn))
            .ToListAsync(ct);

    public async Task<PantryItemDto> AddAsync(AddPantryItem command, CancellationToken ct)
    {
        var name = Ingredient.Normalize(command.Ingredient);
        var ingredient = await db.Ingredients.SingleOrDefaultAsync(i => i.Name == name, ct)
            ?? new Ingredient(name);

        var item = new PantryItem(ingredient, command.Quantity, command.Unit, command.ExpiresOn, clock.GetUtcNow());
        db.PantryItems.Add(item);
        await db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<PantryItemDto?> SetQuantityAsync(int id, decimal quantity, CancellationToken ct)
    {
        var item = await FindAsync(id, ct);
        if (item is null) return null;
        item.SetQuantity(quantity);
        await db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<PantryItemDto?> UseAsync(int id, decimal amount, CancellationToken ct)
    {
        var item = await FindAsync(id, ct);
        if (item is null) return null;
        item.Consume(amount);
        await db.SaveChangesAsync(ct);
        return ToDto(item);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await db.PantryItems.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0;

    private Task<PantryItem?> FindAsync(int id, CancellationToken ct) =>
        db.PantryItems.Include(p => p.Ingredient).SingleOrDefaultAsync(p => p.Id == id, ct);

    private static PantryItemDto ToDto(PantryItem p) =>
        new(p.Id, p.Ingredient.Name, p.Quantity, p.Unit, p.ExpiresOn);
}