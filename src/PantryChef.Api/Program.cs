using PantryChef.Api.Diagnostics;
using PantryChef.Api.Pantry;
using PantryChef.Api.Recipes;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<SingletonOp>();
builder.Services.AddScoped<ScopedOp>();
builder.Services.AddTransient<TransientOp>();
builder.Services.AddTransient<LifetimeReporter>();

builder.Services.AddSingleton<IPantryService, InMemoryPantryService>();
builder.Services.AddOptions<RecipeOptions>()
    .Bind(builder.Configuration.GetSection(RecipeOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// ---- Middleware A (outermost) ----
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    app.Logger.LogInformation("A --> {Method} {Path}", context.Request.Method, context.Request.Path);

    if (context.Request.Path == "/blocked")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        app.Logger.LogInformation("A <-- short-circuited with 403");
        return;  // never call next: B and the endpoint don't run
    }

    await next(context);

    app.Logger.LogInformation("A <-- {Status} in {Ms} ms", context.Response.StatusCode, sw.ElapsedMilliseconds);
});

// ---- Middleware B ----
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    app.Logger.LogInformation("  B: matched endpoint = {Endpoint}", endpoint?.DisplayName ?? "(none)");
    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "PantryChef is running");

app.MapGet("/debug/lifetimes", (SingletonOp s, ScopedOp sc, TransientOp t, LifetimeReporter reporter) => new
{
    Endpoint = new { Singleton = s.Id, Scoped = sc.Id, Transient = t.Id },
    Reporter = reporter.Report()
});

app.MapGet("/pantry", (IPantryService pantry) => pantry.GetAll());

app.MapPost("/pantry", (PantryItem item, IPantryService pantry) =>
{
    var added = pantry.Add(item);
    return Results.Created($"/pantry/{added.Name}", added);
});

app.MapGet("/debug/config", (IConfiguration config, IWebHostEnvironment env) => new
{
    Environment = env.EnvironmentName,
    Greeting = config["PantryChef:Greeting"]
});

app.MapGet("/debug/options", (IOptions<RecipeOptions> opts) => new
{
    opts.Value.Model,
    opts.Value.MaxSuggestions,
    ApiKeyConfigured = !string.IsNullOrEmpty(opts.Value.ApiKey)
});

app.MapGet("debug/options-snapshot", (IOptionsSnapshot<RecipeOptions> opts) => new
{
    opts.Value.Model,
    opts.Value.MaxSuggestions
});

app.Run();