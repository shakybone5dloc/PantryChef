using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using PantryChef.Application.Abstractions;
using PantryChef.Application.Recipes;
using PantryChef.Infrastructure.AI;
using PantryChef.Infrastructure.Data;
using PantryChef.Infrastructure.Identity;

namespace PantryChef.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //----- DataBase -----
        services.AddDbContext<PantryDbContext>(o => o.UseNpgsql(
            configuration.GetConnectionString("pantry")
                ?? throw new InvalidOperationException("Connection string 'Pantry' is not configured.")));
        services.AddIdentityCore<AppUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredLength = 8;
            o.Lockout.MaxFailedAccessAttempts = 5;
        })
            .AddEntityFrameworkStores<PantryDbContext>()
            .AddSignInManager();
        services.AddScoped<IPantryDbContext>(sp => sp.GetRequiredService<PantryDbContext>());

        // ----- AI -----
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var ai = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();

        services.AddHttpClient("ollama", client =>
        {
            client.BaseAddress = ai.Endpoint;
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
            .AddStandardResilienceHandler(o =>
            {
                o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(ai.AttemptTimeoutSeconds);
                o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(ai.AttemptTimeoutSeconds * 3);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(ai.AttemptTimeoutSeconds * 2);
                o.Retry.MaxRetryAttempts = 2;
            });

        services.AddChatClient(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ollama");
            return new OllamaApiClient(http, ai.Model);
        })
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = ai.LogSensitiveData)
            .UseLogging();

        services.AddScoped<IRecipeGenerator, AiRecipeGenerator>();

        return services;
    }
}