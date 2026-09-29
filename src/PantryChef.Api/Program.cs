using PantryChef.Api.Diagnostics;
using PantryChef.Api.Errors;
using PantryChef.Api.Pantry;
using PantryChef.Api.Recipes;
using PantryChef.Api.Auth;
using PantryChef.Api.Background;
using PantryChef.Api.Notifications;
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
builder.Services.AddExceptionHandler<RecipeGenerationExceptionHandler>();

// ---------- Layers ----------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
// ---------- Lesson demos ----------
builder.Services.AddSingleton<SingletonOp>();
builder.Services.AddScoped<ScopedOp>();
builder.Services.AddTransient<TransientOp>();
builder.Services.AddTransient<LifetimeReporter>();
// ---------- Background work ----------
builder.Services.AddOptions<ExpiryDigestOptions>()
    .Bind(builder.Configuration.GetSection(ExpiryDigestOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<ExpiryDigestWorker>();

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
app.MapRecipeEndpoints();
app.MapAuthEndpoints();
app.MapNotificationEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program;