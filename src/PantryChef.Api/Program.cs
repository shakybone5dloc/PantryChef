using Microsoft.EntityFrameworkCore;
using PantryChef.Api.Data;
using PantryChef.Api.Diagnostics;
using PantryChef.Api.Pantry;
using PantryChef.Api.Recipes;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// API plumbing
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DatabaseExceptionHandler>();

// Configuration
builder.Services.AddOptions<RecipeOptions>()
    .Bind(builder.Configuration.GetSection(RecipeOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Data
builder.Services.AddDbContext<PantryDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Pantry")));
builder.Services.AddScoped<IPantryService, EfPantryService>();

// Lesson demos
builder.Services.AddSingleton<SingletonOp>();
builder.Services.AddScoped<ScopedOp>();
builder.Services.AddTransient<TransientOp>();
builder.Services.AddTransient<LifetimeReporter>();

var app = builder.Build();

// Pipeline
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

// Endpoints
app.MapGet("/", () => "PantryChef is running");
app.MapPantryEndpoints();

app.Run();