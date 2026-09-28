using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Application.Abstractions;
using PantryChef.Infrastructure.Data;

namespace PantryChef.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PantryDbContext>(o => o.UseNpgsql(connectionString));

        services.AddScoped<IPantryDbContext>(sp => sp.GetRequiredService<PantryDbContext>());

        return services;
    }
}