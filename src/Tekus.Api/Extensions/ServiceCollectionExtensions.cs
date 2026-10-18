using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Tekus.Api.Middleware;
using Tekus.Api.Security;
using Tekus.Application.Abstractions.Security;
using Tekus.Infrastructure.Persistence;

namespace Tekus.Api.Extensions;

/// <summary>
/// Composition of the API host: OpenAPI (with the JWT security scheme), CORS for the SPA,
/// rate limiting on the sign-in endpoint, health checks and the ProblemDetails pipeline.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>Origin of the React dev server (Vite).</summary>
    private const string SpaDevOrigin = "http://localhost:5173";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Bridge between the HTTP request (JWT claims) and the application layer.
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddApiOpenApi();
        services.AddApiCors(environment);
        services.AddApiRateLimiting();
        services.AddApiHealthChecks();

        return services;
    }

    private static void AddApiOpenApi(this IServiceCollection services)
    {
        var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, "Tekus.Api.xml");

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Tekus Provider Services API",
                Version = "v1",
                Description =
                    "REST API to manage providers and their service catalogue, with custom fields, "
                    + "countries sourced from an external service, JWT authentication, pagination, "
                    + "ordering, search and dashboard indicators.",
            });

            if (File.Exists(xmlCommentsPath))
            {
                options.IncludeXmlComments(xmlCommentsPath);
            }

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the access token returned by POST /api/v1/auth/login.",
                });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    Array.Empty<string>()
                },
            });
        });
    }

    private static void AddApiCors(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Spa", policy =>
            {
                policy.WithOrigins(SpaDevOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

            if (environment.IsDevelopment())
            {
                // In development the SPA may be served from another port or host.
                options.AddPolicy("SpaDevelopment", policy => policy
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }
        });
    }

    private static void AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Rejected requests still get a machine readable body.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("auth", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                    }));
        });
    }

    private static void AddApiHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<TekusDbContext>(
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db"]);
    }
}
