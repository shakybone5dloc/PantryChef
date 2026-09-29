using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using PantryChef.Application.Notifications;

namespace PantryChef.Api.Background;

public sealed class ExpiryDigestOptions
{
    public const string SectionName = "ExpiryDigest";

    public bool Enabled { get; set; } = true;

    [Range(typeof(TimeSpan), "00:00:10", "7.00:00:00")]
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(24);

    [Range(0, 14)]
    public int DaysAhead { get; set; } = 2;
}

public sealed class ExpiryDigestWorker(
    IServiceScopeFactory scopes,
    TimeProvider clock,
    IOptions<ExpiryDigestOptions> options,
    ILogger<ExpiryDigestWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.Enabled)
        {
            logger.LogInformation("Expiry digest is disabled");
            return;
        }

        logger.LogInformation("Expiry digest runs every {Interval}", opts.Interval);
        using var timer = new PeriodicTimer(opts.Interval, clock);

        do
        {
            await RunOnceAsync(opts.DaysAhead, stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(int daysAhead, CancellationToken ct)
    {
        try
        {
            // A fresh scope per run, just like a request: a new DbContext each time
            await using var scope = scopes.CreateAsyncScope();
            var digest = scope.ServiceProvider.GetRequiredService<IExpiryDigestService>();

            var created = await digest.RunAsync(daysAhead, ct);
            logger.LogInformation("Expiry digest created {Count} notification(s)", created);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Expiry digest run failed; will try on the next tick");
        }
    }
}