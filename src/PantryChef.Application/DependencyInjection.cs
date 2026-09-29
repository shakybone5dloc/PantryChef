using Microsoft.Extensions.DependencyInjection;
using PantryChef.Application.Notifications;
using PantryChef.Application.Pantry;
using PantryChef.Application.Recipes;

namespace PantryChef.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPantryService, PantryService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IExpiryDigestService, ExpiryDigestService>();
        return services;
    }
}