using Microsoft.AspNetCore.Http.HttpResults;
using PantryChef.Application.Notifications;

namespace PantryChef.Api.Notifications;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapGet("/", GetRecent);
        group.MapPost("/{id:int}/read", MarkRead);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<NotificationDto>>> GetRecent(
        INotificationService notifications, CancellationToken ct) =>
        TypedResults.Ok(await notifications.GetRecentAsync(ct));

    private static async Task<Results<NoContent, NotFound>> MarkRead(
        int id, INotificationService notifications, CancellationToken ct) =>
        await notifications.MarkReadAsync(id, ct) ? TypedResults.NoContent() : TypedResults.NotFound();
}