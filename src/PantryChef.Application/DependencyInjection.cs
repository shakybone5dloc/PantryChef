using Microsoft.Extensions.DependencyInjection;
using PantryChef.Application.Pantry;

namespace PantryChef.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPantryService, PantryService>();
        return services;
    }
}