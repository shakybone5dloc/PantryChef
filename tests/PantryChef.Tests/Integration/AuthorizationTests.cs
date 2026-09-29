using System.Net;
using System.Net.Http.Json;
using PantryChef.Application.Pantry;
using Xunit;

namespace PantryChef.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public sealed class AuthorizationTests(PantryApiFactory factory) : IntegrationTest(factory)
{
    private async Task<PantryItemDto> AliceAddsEggs() =>
        (await (await Client.PostAsJsonAsync("/pantry", new { ingredient = "eggs", quantity = 12, unit = "count" }, Ct))
            .Content.ReadFromJsonAsync<PantryItemDto>(Ct))!;

    [Fact]
    public async Task Pantry_WithoutToken_Returns401()
    {
        var response = await CreateAnonymousClient().GetAsync("/pantry", Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await CreateAnonymousClient().PostAsJsonAsync(
            "/auth/login", new { email = "alice@test.local", password = "nope-nope-nope" }, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_CannotSeeEachOthersItems()
    {
        await AliceAddsEggs();
        var bob = await CreateUserClientAsync("bob@test.local");

        var bobsItems = await bob.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct);
        var alicesItems = await Client.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct);

        Assert.Empty(bobsItems!);
        Assert.Single(alicesItems!);
    }

    [Fact]
    public async Task Users_CannotChangeOrDeleteEachOthersItems()
    {
        var eggs = await AliceAddsEggs();
        var bob = await CreateUserClientAsync("bob@test.local");

        var patch = await bob.PatchAsJsonAsync($"/pantry/{eggs.Id}", new { quantity = 0 }, Ct);
        var delete = await bob.DeleteAsync($"/pantry/{eggs.Id}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);    // 404, not 403: don't confirm it exists
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        var alicesItems = await Client.GetFromJsonAsync<List<PantryItemDto>>("/pantry", Ct);
        Assert.Equal(12m, Assert.Single(alicesItems!).Quantity);
    }

    [Fact]
    public async Task Recipes_UseOnlyTheCallersPantry()
    {
        await AliceAddsEggs();
        var bob = await CreateUserClientAsync("bob@test.local");

        var response = await bob.PostAsJsonAsync("/recipes/suggest", new { count = 1 }, Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);   // Bob's pantry is empty
    }
}