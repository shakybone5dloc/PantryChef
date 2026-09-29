using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Application.Notifications;
using Xunit;

namespace PantryChef.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ExpiryDigestTests(PantryApiFactory factory) : IntegrationTest(factory)
{
    private Task<HttpResponseMessage> Add(HttpClient client, string name, string? expiresOn) =>
        client.PostAsJsonAsync("/pantry", new { ingredient = name, quantity = 1, unit = "count", expiresOn }, Ct);

    private async Task<int> RunDigest()
    {
        // Same as the worker: a fresh scope, and no HTTP request, so no current user
        await using var scope = Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IExpiryDigestService>().RunAsync(daysAhead: 2, Ct);
    }

    [Fact]
    public async Task Digest_NotifiesEachOwner_OnlyAboutTheirOwnExpiringItems()
    {
        // The fake clock says "today" is 2026-10-01
        await Add(Client, "spinach", "2026-10-02");  // soon    -> included
        await Add(Client, "milk", "2026-09-30");     // expired -> skipped
        await Add(Client, "rice", "2026-12-01");     // Far Off -> skipped
        var bob = await CreateUserClientAsync("bob@test.local");
        await Add(bob, "yogurt", "2026-10-03");

        var created = await RunDigest();

        Assert.Equal(2, created);  // one for Alice, one for Bob
        var alices = Assert.Single((await Client.GetFromJsonAsync<List<NotificationDto>>("/notifications", Ct))!);
        Assert.Contains("spinach", alices.Message);
        Assert.DoesNotContain("milk", alices.Message);
        Assert.DoesNotContain("rice", alices.Message);
        Assert.DoesNotContain("yogurt", alices.Message);   // Bob's item never leaks into Alice's digest
    }

    [Fact]
    public async Task Digest_IsIdempotent_WithinTheSameDay()
    {
        await Add(Client, "spinach", "2026-10-02");

        Assert.Equal(1, await RunDigest());
        Assert.Equal(0, await RunDigest());
    }
}