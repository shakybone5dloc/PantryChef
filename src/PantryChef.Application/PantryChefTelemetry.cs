using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PantryChef.Application;

public static class PantryChefTelemetry
{
    public const string Name = "PantryChef";

    public static readonly ActivitySource ActivitySource = new(Name);
    private static readonly Meter Meter = new(Name);

    public static readonly Counter<long> RecipeRequests =
        Meter.CreateCounter<long>("pantrychef.recipes.requests", description: "Recipe suggestion requests, by outcome");

    public static readonly Histogram<double> RecipeGenerationDuration =
        Meter.CreateHistogram<double>("pantrychef.recipes.generation.duration", unit: "s",
            description: "Time spent generating recipe suggestions");

    public static readonly Counter<long> DigestNotifications =
        Meter.CreateCounter<long>("pantrychef.digest.notifications", description: "Expiry notifications created");
}