using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tekus.Application.Abstractions.Countries;
using Tekus.Application.Abstractions.Persistence;
using Tekus.Application.Abstractions.Security;
using Tekus.Domain.Abstractions;
using Tekus.Domain.Common;
using Tekus.Domain.Countries;
using Tekus.Domain.Providers;
using Tekus.Domain.Services;
using Tekus.Domain.Users;
using Tekus.Infrastructure.Countries;
using Tekus.Infrastructure.Messaging;
using Tekus.Infrastructure.Persistence;
using Tekus.Infrastructure.Persistence.Repositories;
using Tekus.Infrastructure.Persistence.Seeding;
using Tekus.Infrastructure.Security;

namespace Tekus.Infrastructure;

/// <summary>
/// Composition root of the infrastructure layer: EF Core, the external country catalogue, JWT
/// authentication and the development seed. The API project only calls
/// <see cref="AddInfrastructure"/> and <see cref="UseInfrastructureAuthentication"/>, so swapping a
/// concrete adapter (e.g. another country provider) is a one-line change here.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Section of appsettings.json holding the JWT settings.</summary>
    public const string JwtSection = JwtOptions.SectionName;

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Default")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' is missing from the configuration.");

        services.AddDbContext<TekusDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                );

        // A single, injectable clock for the whole system: every timestamp is UTC and tests can
        // freeze it. See docs/architecture.md ("Time").
        services.TryAddSingleton(TimeProvider.System);

        // --- Security -------------------------------------------------------
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.TryAddSingleton<IPasswordHasher, PasswordHasher>();
        services.TryAddSingleton<ITokenService, TokenService>();

        // --- Persistence ----------------------------------------------------
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.TryAddScoped<IProviderRepository, ProviderRepository>();
        services.TryAddScoped<IServiceRepository, ServiceRepository>();
        services.TryAddScoped<ICountryRepository, CountryRepository>();
        services.TryAddScoped<IUserRepository, UserRepository>();

        // --- Read models (CQRS query side) ----------------------------------
        services.TryAddScoped<IProviderReadRepository, ProviderReadRepository>();
        services.TryAddScoped<IServiceReadRepository, ServiceReadRepository>();
        services.TryAddScoped<ICountryReadRepository, CountryReadRepository>();
        services.TryAddScoped<IDashboardReadRepository, DashboardReadRepository>();

        // --- External country catalogue -------------------------------------
        services.Configure<CountryCatalogOptions>(configuration.GetSection(CountryCatalogOptions.SectionName));

        services
            .AddHttpClient<ICountryCatalog, RestCountriesCountryCatalog>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CountryCatalogOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TekusProviderServices/1.0");
            })
            // Standard resilience adds retry with exponential backoff, a circuit breaker and a
            // total timeout, so a flaky external API never hangs a request thread.
            .AddStandardResilienceHandler();

        // --- Startup --------------------------------------------------------
        services.TryAddScoped<DatabaseBootstrapper>();
        services.TryAddScoped<DevelopmentSeeder>();

        return services;
    }

    /// <summary>
    /// Configures JWT bearer authentication using the same <see cref="JwtOptions"/> the token
    /// service uses to sign, so signing and validation can never disagree.
    /// </summary>
    public static AuthenticationBuilder UseInfrastructureAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException(
                $"Configuration section '{JwtOptions.SectionName}:SigningKey' is required. " +
                "Set it in appsettings.json, user-secrets or an environment variable.");
        }

        return services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
    }

    /// <summary>
    /// Applies the pending EF Core migrations and (optionally) runs the development seed.
    /// Called once at startup by the API host. It is exposed as an extension on
    /// <see cref="IServiceProvider"/> so the internal bootstrapper stays an implementation detail.
    /// </summary>
    /// <param name="seedDevelopmentData">
    /// When <c>true</c> the deterministic demo data (users, countries, services and providers) is
    /// inserted if the tables are empty. Enable it only in the Development environment.
    /// </param>
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        bool seedDevelopmentData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();

        var bootstrapper = scope.ServiceProvider.GetRequiredService<DatabaseBootstrapper>();

        await bootstrapper.InitializeAsync(seedDevelopmentData, cancellationToken);
    }
}
