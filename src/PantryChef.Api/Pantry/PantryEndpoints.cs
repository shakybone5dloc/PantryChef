using Microsoft.AspNetCore.Http.HttpResults;

namespace PantryChef.Api.Pantry;

public static class PantryEndpoints
{
    public static IEndpointRouteBuilder MapPantryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pantry").WithTags("Pantry");

        group.MapGet("/", GetAll);
        group.MapPost("/", Add);
        group.MapPatch("/{id:int}", UpdateQuantity);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    public static async Task<Ok<IReadOnlyList<PantryItemResponse>>> GetAll(
        IPantryService pantry, CancellationToken ct) =>
            TypedResults.Ok(await pantry.GetAllAsync(ct));

    public static async Task<Created<PantryItemResponse>> Add(
        AddPantryItemRequest request, IPantryService pantry, CancellationToken ct)
    {
        var added = await pantry.AddAsync(request, ct);
        return TypedResults.Created($"/pantry/{added.Id}", added);
    }

    private static async Task<Results<Ok<PantryItemResponse>, NotFound>> UpdateQuantity(
        int id, UpdateQuantityRequest request, IPantryService pantry, CancellationToken ct) =>
        await pantry.UpdateQuantityAsync(id, request.Quantity, ct) is { } updated
            ? TypedResults.Ok(updated)
            : TypedResults.NotFound();

    private static async Task<Results<NoContent, NotFound>> Delete(
        int id, IPantryService pantry, CancellationToken ct) =>
        await pantry.DeleteAsync(id, ct) ? TypedResults.NoContent() : TypedResults.NotFound();
}