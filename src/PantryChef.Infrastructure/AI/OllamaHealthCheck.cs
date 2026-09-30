using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace PantryChef.Infrastructure.AI;

public sealed class OllamaHealthCheck(IHttpClientFactory factory, IOptions<AiOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            // A plain client with a short timeout. NOT the "ollama" client, whose retries and
            // 120-second timeouts would make a health probe hang
            using var client = factory.CreateClient("ollama-health");
            client.BaseAddress = options.Value.Endpoint;
            client.Timeout = TimeSpan.FromSeconds(3);

            using var response = await client.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded($"ollama returned {(int)response.StatusCode}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return HealthCheckResult.Degraded("Ollama is unreachable; recipe suggestions are unavailable.", ex);
        }
    }
}