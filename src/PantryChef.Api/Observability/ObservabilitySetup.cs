using System.Diagnostics;
using System.Security.Claims;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PantryChef.Application;

namespace PantryChef.Api.Observability;

public static class ObservabilitySetup
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(o =>
        {
            o.IncludeFormattedMessage = true;
            o.IncludeScopes = true;
        });

        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("pantrychef-api"))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Npgsql")
                .AddSource("*Microsoft.Extensions.AI")
                .AddSource(PantryChefTelemetry.Name))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("System.Runtime")
                .AddMeter("Npgsql")
                .AddMeter("Polly")
                .AddMeter("*Microsoft.Extensions.AI")
                .AddMeter(PantryChefTelemetry.Name));

        // Only export when an endpoint is configured (dev dasboard, prod collector); tests stay quiet
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            otel.UseOtlpExporter();

        return builder;
    }

    public static IApplicationBuilder UseUserTagging(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (context.User.FindFirstValue("sub") is { } userId)
                Activity.Current?.SetTag("user.id", userId);
            await next(context);
        });

}