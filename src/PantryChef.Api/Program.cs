using PantryChef.Api.Diagnostics;
using PantryChef.Api.Errors;
using PantryChef.Api.Pantry;
using PantryChef.Api.Recipes;
using PantryChef.Application;
using PantryChef.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- API plumbing ----------
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();    // handlers are tried in order
builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();

// ---------- Configuration ----------
builder.Services.AddOptions<RecipeOptions>()
    .Bind(builder.Configuration.GetSection(RecipeOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ---------- Layers ----------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Pantry")
        ?? throw new InvalidOperationException("Connection string 'Pantry' is not configured."));

// ---------- Lesson demos ----------
builder.Services.AddSingleton<SingletonOp>();
builder.Services.AddScoped<ScopedOp>();
builder.Services.AddTransient<TransientOp>();
builder.Services.AddTransient<LifetimeReporter>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next(context);
    app.Logger.LogInformation("{Method} {Path} -> {Status} in {Ms} ms",
        context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapDebugEndpoints();
}

app.MapGet("/", () => "PantryChef is running");
app.MapPantryEndpoints();

app.Run();