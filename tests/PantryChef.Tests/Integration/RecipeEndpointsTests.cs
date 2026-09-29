using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PantryChef.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RecipeEndpointsTests(PantryApiFactory factory) : IntegrationTest(factory)
{
    private Task<HttpResponseMessage> Add(string name, string? expiresOn) =>
        Client.PostAsJsonAsync("/pantry", new { ingredient = name, quantity = 1, unit = "bag", expiresOn }, Ct);

    private Task<HttpResponseMessage> Suggest() =>
        Client.PostAsJsonAsync("/recipes/suggest", new { count = 1 }, Ct);

    [Fact]
    public async Task Suggest_EmptyPantry_Returns422()
    {
        var response = await Suggest();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Suggest_SkipsExpired_AndFlagsExpiringSoon()
    {
        // The fake clock says "today" is 2026-10-01
        await Add("milk", "2026-09-30");      // expired yesterday → excluded
        await Add("spinach", "2026-10-03");   // within 3 days     → flagged
        await Add("rice", null);              // never expires     → not flagged

        var response = await Suggest();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var prompt = Factory.Chat.LastMessages[^1].Text;
        Assert.DoesNotContain("milk", prompt);
        Assert.Contains("spinach: 1.00 bag (expires soon)", prompt);
        Assert.Contains("rice: 1.00 bag", prompt);
        Assert.DoesNotContain("rice: 1.00 bag (expires soon)", prompt);
    }

    [Fact]
    public async Task Suggest_AiDown_Returns503ProblemDetails()
    {
        await Add("rice", null);
        Factory.Chat.ExceptionToThrow = new HttpRequestException("Ollama is down");

        var response = await Suggest();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}