using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using PantryChef.Api.Background;
using PantryChef.Application.Notifications;
using Xunit;

namespace PantryChef.Tests.Unit.Background;

public class ExpiryDigestWorkerTests
{
    private sealed class CountingDigest : IExpiryDigestService
    {
        private int _runs;
        public int Runs => Volatile.Read(ref _runs);

        public Task<int> RunAsync(int dayAhead, CancellationToken ct)
        {
            Interlocked.Increment(ref _runs);  // the worker runs on another thread
            return Task.FromResult(0);
        }
    }

    [Fact]
    public async Task RunsAtStartup_ThenOncePerInterval()
    {
        var ct = TestContext.Current.CancellationToken;
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 1, 7, 0, 0, TimeSpan.Zero));
        var digest = new CountingDigest();
        var services = new ServiceCollection().AddSingleton<IExpiryDigestService>(digest).BuildServiceProvider();

        var worker = new ExpiryDigestWorker(
            services.GetRequiredService<IServiceScopeFactory>(),
            clock,
            Options.Create(new ExpiryDigestOptions { Interval = TimeSpan.FromHours(24) }),
            NullLogger<ExpiryDigestWorker>.Instance);

        await worker.StartAsync(ct);
        await WaitUntil(() => digest.Runs == 1);   // immediate first run

        clock.Advance(TimeSpan.FromHours(23));
        await Task.Delay(100, ct);
        Assert.Equal(1, digest.Runs);               // not a full day yet

        clock.Advance(TimeSpan.FromHours(1));
        await WaitUntil(() => digest.Runs == 2);    // exactly on day later

        await worker.StopAsync(ct);                 // graceful shutdown; must not throw
    }

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 2000)
    {
        var sw = Stopwatch.StartNew();
        while (!condition())
        {
            if (sw.ElapsedMilliseconds > timeoutMs) throw new TimeoutException("Condition not met in time.");
            await Task.Delay(10);
        }
    }
}