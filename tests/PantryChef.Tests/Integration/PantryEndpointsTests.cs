using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Application.Pantry;
using PantryChef.Domain.Pantry;
using PantryChef.Infrastructure.Data;
using PantryChef.Contracts;
using Xunit;

namespace PantryChef.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public sealed class PantryEndpointsTests(PantryApiFactory factory) : IntegrationTest(factory)
{
    [Fact]
    public async Task Add_ThenGetAll_ReturnsNormalizedItem()
    {
        var post = await Client.PostAsJsonAsync("/pantry", new { ingredient = "  Eggs ", quantity = 12, unit = "Count" }, Ct);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var items = await Client.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct);

        var item = Assert.Single(items!);
        Assert.Equal("eggs", item.Ingredient);
        Assert.Equal("count", item.Unit);
        Assert.Equal(12m, item.Quantity);
    }

    [Fact]
    public async Task Add_SameIngredientTwice_ReuseIngredientRow()
    {
        await Client.PostAsJsonAsync("/pantry", new { ingredient = "eggs", quantity = 12, unit = "count" }, Ct);
        await Client.PostAsJsonAsync("/pantry", new { ingredient = "eggs", quantity = 6, unit = "count" }, Ct);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PantryDbContext>();
        Assert.Equal(1, await db.Ingredients.CountAsync(Ct));
        Assert.Equal(2, await db.PantryItems.IgnoreQueryFilters().CountAsync(Ct));
    }

    [Fact]
    public async Task Add_InvalidRequest_Returns400WithFieldErrors()
    {
        var response = await Client.PostAsJsonAsync("/pantry", new { ingredient = "", quantity = -5, unit = "count" }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(Ct);
        Assert.Contains("Quantity", problem!.Errors.Keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Use_MoreThanAvailable_Returns422()
    {
        var created = await (await Client.PostAsJsonAsync("/pantry", new { ingredient = "eggs", quantity = 3, unit = "count" }, Ct))
            .Content.ReadFromJsonAsync<PantryItemDto>(Ct);

        var response = await Client.PostAsJsonAsync($"/pantry/{created!.Id}/use", new { amount = 5 }, Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Patch_UnknownId_Returns404()
    {
        var response = await Client.PatchAsJsonAsync("/pantry/999", new { quantity = 1 }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReflectsWritesImmediately_CacheIsInvalidated()
    {
        await Client.PostAsJsonAsync("/pantry", new { Ingredient = "eggs", quantity = 12, unit = "count" }, Ct);
        Assert.Single((await Client.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct))!);

        await Client.PostAsJsonAsync("/pantry", new { ingredient = "rice", quantity = 2, unit = "lb" }, Ct);

        Assert.Equal(2, (await Client.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct))!.Count);
    }
}

