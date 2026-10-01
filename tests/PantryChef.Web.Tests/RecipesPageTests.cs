using System.Net;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Contracts;
using PantryChef.Web.Pages;
using Xunit;

namespace PantryChef.Web.Tests;

public class RecipesPageTests : BunitContext
{
    private void UseApi(StubHttpHandler handler) => Services.AddSingleton(handler.CreateClient());

    [Fact]
    public void ShowsRecipeCards()
    {
        UseApi(StubHttpHandler.Json(new[]
        {
            new RecipeSuggestion("Spinach Rice", "Easy weeknight bowl.", 20,
                [new RecipeIngredient("rice", "1 cup")], ["Cook the rice."], [])
        }));
        var cut = Render<Recipes>();

        cut.Find("button.btn-primary").Click();

        cut.WaitForAssertion(() => Assert.Equal("Spinach Rice", cut.Find(".card-title").TextContent));
    }

    [Fact]
    public void ShowsAFriendlyMessage_WhenTheAiIsDown()
    {
        UseApi(StubHttpHandler.Json(
            new { title = "Recipe service unavailable", detail = "The recipe AI is unavailable right now." },
            HttpStatusCode.ServiceUnavailable));
        var cut = Render<Recipes>();

        cut.Find("Button.btn-primary").Click();

        cut.WaitForAssertion(() => Assert.Contains("Your pantry still works", cut.Find(".alert").TextContent));
    }

    [Fact]
    public void Cancel_StopsTheRequest()
    {
        UseApi(new StubHttpHandler(async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return new HttpResponseMessage();
        }));
        var cut = Render<Recipes>();

        cut.Find("button.btn-primary").Click();
        cut.Find("button.btn-outline-secondary").Click();

        cut.WaitForAssertion(() => Assert.Equal("Cancelled.", cut.Find(".alert").TextContent.Trim()));
    }
}