using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PantryChef.Api.Data;
using PantryChef.Api.Recipes;

namespace PantryChef.Api.Diagnostics;

public static class DebigEndpoints
{
    public static IEndpointRouteBuilder MapDebugEndpoints(this IEndpointRouteBuilder app)
    {
        var debug = app.MapGroup("/debug").WithTags("debug");

        debug.MapGet("/config", (IConfiguration config, IWebHostEnvironment env) => new
        {
            Environment = env.EnvironmentName,
            Greeting = config["PantryChef:Greeting"]
        });

        debug.MapGet("/lifetimes", (SingletonOp s, ScopedOp sc, TransientOp t, LifetimeReporter reporter) => new
        {
            Endpoint = new { Singleton = s.Id, Scoped = sc.Id, Transient = t.Id },
            Reporter = reporter.Report()
        });

        debug.MapGet("/options", (IOptions<RecipeOptions> o) => new
        {
            o.Value.Model, o.Value.MaxSuggestions, ApiKeyConfigured = !string.IsNullOrEmpty(o.Value.ApiKey)
        });

        debug.MapGet("/options-snapshot", (IOptionsSnapshot<RecipeOptions> o) => new { o.Value.Model, o.Value.MaxSuggestions });

        debug.MapGet("/tracking/{id:int}", async (int id, PantryDbContext db) =>
        {
            var item = await db.PantryItems.SingleAsync(p => p.Id == id);
            var before = db.Entry(item).State.ToString();
            item.Quantity += 1;
            db.ChangeTracker.DetectChanges();
            return new { before, after = db.Entry(item).State.ToString(), tracker = db.ChangeTracker.DebugView.LongView };
        });

        debug.MapGet("/ingredients-slow", async (PantryDbContext db) =>
        {
            var ingredients = await db.Ingredients.ToListAsync();
            var result = new List<object>();
            foreach (var ing in ingredients)
                result.Add(new { ing.Name, Count = await db.PantryItems.CountAsync(p => p.IngredientId == ing.Id) });
            return result;
        });

        debug.MapGet("/ingredients-fast", (PantryDbContext db) =>
            db.Ingredients.Select(i => new { i.Name, Count = i.PantryItems.Count }).ToListAsync());

        debug.MapGet("/threads", async () =>
        {
            var before = Environment.CurrentManagedThreadId;
            await Task.Delay(100);
            var after = Environment.CurrentManagedThreadId;
            return new { before, after, poolthreads = ThreadPool.ThreadCount };
        });

        debug.MapGet("/wait-async", async () => { await Task.Delay(1000); return "done"; });
        debug.MapGet("/wait-blocking", () => { ThreadSleep(1000); return "done"; });

        debug.MapGet("/slow", async (CancellationToken ct, ILoggerFactory lf) =>
        {
            var log = lf.CreateLogger("Debug.Slow");
            try
            {
                await Task.Delay(10_000, ct);
                return "finished";
            }
            catch (OperationCanceledException)
            {
                log.LogWarning("Client disconnected after less than 10s, so we stopped the work");
                throw;
            }
        });

        return app;
    }
}