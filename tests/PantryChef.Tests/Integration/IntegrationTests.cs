using Xunit;

namespace PantryChef.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<PantryApiFactory>
{
    public const string Name = "api";
}

public abstract class IntegrationTest(PantryApiFactory factory) : IAsyncLifetime
{
    protected PantryApiFactory Factory { get; } = factory;
    protected HttpClient Client { get; } = factory.CreateClient();
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await Factory.ResetAsync();

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }
}