using System.Net.Http.Headers;
using System.Net.Http.Json;
using PantryChef.Api.Auth;
using Xunit;

namespace PantryChef.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<PantryApiFactory>
{
    public const string Name = "api";
}

public abstract class IntegrationTest(PantryApiFactory factory) : IAsyncLifetime
{
    protected const string Password = "Test!Passw0rd";
    private readonly List<HttpClient> _clients = [];

    protected PantryApiFactory Factory { get; } = factory;
    protected HttpClient Client { get; private set; } = null!;   // logged in as alice
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Factory.ResetAsync();
        Client = await CreateUserClientAsync("alice@test.local");
    }

    protected HttpClient CreateAnonymousClient()
    {
        var client = Factory.CreateClient();
        _clients.Add(client);
        return client;
    }

    protected async Task<HttpClient> CreateUserClientAsync(string email)
    {
        var client = CreateAnonymousClient();
        (await client.PostAsJsonAsync("/auth/register", new { email, password = Password }, Ct)).EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/auth/login", new { email, password = Password }, Ct);
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<AccessToken>(Ct))!.Token;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public ValueTask DisposeAsync()
    {
        foreach (var c in _clients) c.Dispose();
        return ValueTask.CompletedTask;
    }
}