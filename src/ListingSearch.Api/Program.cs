using System.Diagnostics;
using ListingSearch.Api.Infrastructure;
using ListingSearch.Application;
using ListingSearch.Application.Scoring;
using ListingSearch.Infrastructure;
using ListingSearch.Infrastructure.Health;
using ListingSearch.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);

// ---- Layers -------------------------------------------------------------------------------
builder.Services.Configure<ScoringOptions>(builder.Configuration.GetSection(ScoringOptions.SectionName));
builder.Services.Configure<ListingDataOptions>(builder.Configuration.GetSection(ListingDataOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// ---- HTTP ---------------------------------------------------------------------------------
builder.Services.AddControllers(mvc =>
{
    // Default binding messages name the C# property ("MinPrice"); keep them neutral since the error key already names the field.
    mvc.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, _) => $"'{value}' is not a valid value.");
    mvc.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(_ => "The value must be a number.");
});

builder.Services.AddProblemDetails(problem => problem.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
    context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddRequestTimeouts(timeouts => timeouts.DefaultPolicy = new RequestTimeoutPolicy
{
    Timeout = TimeSpan.FromSeconds(10),
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).WithMethods(HttpMethods.Get).AllowAnyHeader()));
}

builder.Services.AddHealthChecks().AddCheck<ListingsLoadedHealthCheck>("listings", tags: ["ready"]);
builder.Services.AddOpenApi();

var app = builder.Build();

// ---- Pipeline (order matters) -------------------------------------------------------------
app.UseExceptionHandler();
app.UseStatusCodePages(); // Bodiless 404/405/504 responses become ProblemDetails too.

if (!app.Environment.IsDevelopment())
{
    // TLS terminates at the load balancer; HSTS tells browsers to stay on HTTPS.
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();

if (allowedOrigins.Length > 0)
{
    app.UseCors();
}

app.UseRateLimiter();
app.UseRequestTimeouts();

app.MapControllers().RequireRateLimiting(RateLimiting.ApiPolicy);

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync();

/// <summary>Exposed so integration tests can host the app with WebApplicationFactory.</summary>
public partial class Program;
