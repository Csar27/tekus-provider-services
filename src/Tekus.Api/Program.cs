using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Tekus.Application;
using Tekus.Api.Extensions;
using Tekus.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Serilog: structured logging configured from appsettings.json (console + file).
// ---------------------------------------------------------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

// ---------------------------------------------------------------------------
// Application + Infrastructure + API services.
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration, builder.Environment);
builder.Services.UseInfrastructureAuthentication(builder.Configuration);

var app = builder.Build();

// ---------------------------------------------------------------------------
// Database: apply migrations (and, in development, seed the demo data) before serving.
// ---------------------------------------------------------------------------
await app.Services.InitializeDatabaseAsync(
    seedDevelopmentData: app.Environment.IsDevelopment(),
    cancellationToken: app.Lifetime.ApplicationStopping);

// ---------------------------------------------------------------------------
// HTTP pipeline.
// ---------------------------------------------------------------------------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tekus Provider Services API v1");
        options.DocumentTitle = "Tekus Provider Services API";
    });
}

app.UseCors(app.Environment.IsDevelopment() ? "SpaDevelopment" : "Spa");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Liveness/readiness probe used by container orchestrators and the health dashboard.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
            }),
        });
    },
});

app.Run();

/// <summary>Exposed for the integration test suite (WebApplicationFactory).</summary>
public partial class Program;
