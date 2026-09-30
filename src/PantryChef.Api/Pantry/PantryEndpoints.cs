using Microsoft.AspNetCore.Http.HttpResults;
using PantryChef.Application.Pantry;
using PantryChef.Contracts;

namespace PantryChef.Api.Pantry;

public static class PantryEndpoints
{
    public static IEndpointRouteBuilder MapPantryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pantry").WithTags("Pantry").RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapPost("/", Add);
        group.MapPatch("/{id:int}", SetQuantity);
        group.MapPost("/{id:int}/use", Use);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<PantryItemDto>>> GetAll(IPantryService pantry, CancellationToken ct) =>
        TypedResults.Ok(await pantry.GetAllAsync(ct));

    private static async Task<Created<PantryItemDto>> Add(
        AddPantryItemRequest request, IPantryService pantry, CancellationToken ct)
    {
        var added = await pantry.AddAsync(
            new AddPantryItem(request.Ingredient, request.Quantity, request.Unit, request.ExpiresOn), ct);
        return TypedResults.Created($"/pantry/{added.Id}", added);
    }

    private static async Task<Results<Ok<PantryItemDto>, NotFound>> SetQuantity(
        int id, UpdateQuantityRequest request, IPantryService pantry, CancellationToken ct) =>
        await pantry.SetQuantityAsync(id, request.Quantity, ct) is { } item
            ? TypedResults.Ok(item) : TypedResults.NotFound();

    private static async Task<Results<Ok<PantryItemDto>, NotFound>> Use(
        int id, UseItemRequest request, IPantryService pantry, CancellationToken ct) =>
        await pantry.UseAsync(id, request.Amount, ct) is { } item
            ? TypedResults.Ok(item) : TypedResults.NotFound();

    private static async Task<Results<NoContent, NotFound>> Delete(
        int id, IPantryService pantry, CancellationToken ct) =>
        await pantry.DeleteAsync(id, ct) ? TypedResults.NoContent() : TypedResults.NotFound();
}