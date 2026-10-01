using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Contracts;
using PantryChef.Web.Layout;
using PantryChef.Web.Services;
using Xunit;

namespace PantryChef.Web.Tests;

public class NotificationBadgeTests : BunitContext
{
    private StubHttpHandler UseNotifications(params NotificationDto[] notifications)
    {
        var handler = StubHttpHandler.Json(notifications);
        Services.AddSingleton(handler.CreateClient());
        Services.AddSingleton<AuthenticationStateProvider, FakeAuthStateProvider>();
        Services.AddSingleton<NotificationState>();
        return handler;
    }

    [Fact]
    public void ShowsOnlyTheUnreadCount()
    {
        UseNotifications(
            new NotificationDto(1, new DateOnly(2026, 10, 1), "Use soon: spinach", DateTimeOffset.UtcNow, IsRead: false),
            new NotificationDto(2, new DateOnly(2026, 9, 30), "Use soon: milk", DateTimeOffset.UtcNow, IsRead: true));

        var cut = Render<NotificationBadge>();

        cut.WaitForAssertion(() => Assert.Equal("1", cut.Find(".badge").TextContent));
    }

    [Fact]
    public void HidesTheBadge_WhenEverythingIsRead()
    {
        var handler = UseNotifications();

        var cut = Render<NotificationBadge>();

        cut.WaitForAssertion(() => Assert.Single(handler.Requests));
        Assert.Empty(cut.FindAll(".badge"));
    }
}