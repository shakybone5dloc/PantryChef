using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using PantryChef.Infrastructure.Data;
using PantryChef.Tests.Fakes;
using Testcontainers.PostgreSql;
using Xunit;

namespace PantryChef.Tests.Integration;

public sealed class PantryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly DateTimeOffset StartTime = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    public FakeChatClient Chat { get; } = new();
    public FakeTimeProvider Clock { get; } = new(StartTime);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Pantry", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", "integration-tests-signing-key-at-least-32-characters-long");
        builder.UseSetting("ExpiryDigest:Enabled", "false");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatClient>();
            services.AddSingleton<IChatClient>(Chat);

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PantryDbContext>().Database.MigrateAsync();
    }

    public async Task ResetAsync()
    {
        Chat.Reset();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PantryDbContext>().Database
            .ExecuteSqlRawAsync("""TRUNCATE "Notifications", "PantryItems", "Ingredients", "AspNetUsers" RESTART IDENTITY CASCADE;""");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}