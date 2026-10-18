using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tekus.Application.Abstractions.Security;
using Tekus.Application.Common;
using Tekus.Domain.Countries;
using Tekus.Domain.Providers;
using Tekus.Domain.Services;
using Tekus.Domain.Users;

namespace Tekus.Infrastructure.Persistence.Seeding;

/// <summary>
/// Deterministic development/demo seed. It runs on startup when the database is empty so a
/// developer (or the reviewer of this challenge) gets a populated API on the first <c>dotnet run</c>,
/// without having to execute the SQL scripts by hand.
/// <para>
/// Everything is created through the domain factories, so the seed data obeys exactly the same
/// invariants (and raises the same domain events → audit rows) as data created through the API.
/// </para>
/// </summary>
internal sealed class DevelopmentSeeder
{
    /// <summary>
    /// Pre-computed <c>PBKDF2-SHA256</c> hash of the shared demo password <c>Tekus.2026!</c>.
    /// It is identical to the value used by <c>db/02_seed_data.sql</c>, so a database seeded by
    /// either path accepts the same credentials. Salt is the ASCII of <c>1234567890abcdef</c>.
    /// </summary>
    public const string DemoPasswordHash =
        "PBKDF2-SHA256$100000$MTIzNDU2Nzg5MGFiY2RlZg==$BqJhyRr44ekSR8RNG1C/VuQ0yQENgPuAUED+TUaJVfQ=";

    public const string DemoPassword = "Tekus.2026!";

    private static readonly (string Code, string Name, string Region, string Currency, string Alpha3)[] SeedCountries =
    [
        ("CO", "Colombia", "Americas", "COP", "COL"),
        ("MX", "Mexico", "Americas", "MXN", "MEX"),
        ("PE", "Peru", "Americas", "PEN", "PER"),
        ("CL", "Chile", "Americas", "CLP", "CHL"),
        ("AR", "Argentina", "Americas", "ARS", "ARG"),
        ("BR", "Brazil", "Americas", "BRL", "BRA"),
        ("US", "United States", "Americas", "USD", "USA"),
        ("ES", "Spain", "Europe", "EUR", "ESP"),
        ("PA", "Panama", "Americas", "PAB", "PAN"),
        ("EC", "Ecuador", "Americas", "USD", "ECU"),
        ("GT", "Guatemala", "Americas", "GTQ", "GTM"),
        ("CR", "Costa Rica", "Americas", "CRC", "CRI"),
    ];

    private static readonly (string Name, decimal Rate, string? Description, string[] Countries)[] SeedServices =
    [
        ("Descarga espacial de contenidos", 120.50m, "Descarga de contenidos binarios a través de enlaces orbitales.", ["CO", "MX", "PE"]),
        ("Teletransporte de materia", 450.00m, "Transporte de materia a larga distancia mediante portales estables.", ["CO", "CL"]),
        ("Análisis de datos interplanetarios", 80.75m, "Procesamiento y análisis de datasets provenientes de sondas.", ["CO", "MX", "PE", "CL", "AR", "BR"]),
        ("Soporte de infraestructura orbital", 210.25m, "Mantenimiento continuo de estaciones y plataformas en órbita.", ["US", "ES"]),
        ("Consultoría en tecnología cuántica", 300.00m, "Asesoría especializada en computación y redes cuánticas.", ["CO", "US"]),
        ("Desaparición forzada de bytes", 95.40m, "Borrado seguro e irreversible de datos sensibles.", ["CO", "PE", "EC"]),
        ("Simulación de agujeros de gusano", 512.30m, "Modelado y simulación de pasarelas topológicas.", ["US"]),
        ("Mantenimiento de estaciones espaciales", 175.00m, "Soporte técnico preventivo y correctivo de estaciones.", ["CO", "MX", "PA", "CR"]),
        ("Traducción simultánea alienígena", 60.90m, "Interpretación en tiempo real de lenguajes no terrestres.", ["ES", "MX", "AR"]),
        ("Seguridad perimetral galáctica", 133.75m, "Vigilancia y defensa de perímetros en el sistema solar.", ["CO", "US", "BR"]),
        ("Transporte de carga lunar", 289.60m, "Logística de carga entre la Tierra y la superficie lunar.", ["CL", "PE", "GT"]),
        ("Renderizado de nebulosas", 45.20m, "Generación de imágenes de alta fidelidad de nebulosas.", ["CO", "AR", "BR", "ES"]),
    ];

    private static readonly (string Nit, string Name, string Email, int[] Services, (string Key, string Value)[] Fields)[] SeedProviders =
    [
        ("900123456-1", "Servicios Integrales Andinos", "contacto@andinos.example", [0, 2, 5],
            [("Número de contacto en marte", "+57 555 0101"), ("Sector", "Gobierno")]),
        ("901234567-2", "Corporación Espacial Ltda", "info@corporacionespacial.example", [0, 1, 7],
            [("Certificación", "ISO-9001")]),
        ("900987654-3", "Soluciones Cuánticas S.A.", "ventas@cuanticas.example", [4, 9],
            [("Número de contacto en marte", "+57 555 0103")]),
        ("830123456-4", "Tecnología Orbital SAS", "hola@orbital.example", [3, 6], []),
        ("900555123-5", "Ingeniería del Vacío S.A.", "contacto@vacio.example", [1, 10], []),
        ("860111222-6", "Logística Interplanetaria", "ops@interplanetaria.example", [7, 10],
            [("Régimen", "Gran contribuyente")]),
        ("901444888-7", "BioSistemas Andinos", "info@biosistemas.example", [2, 8], []),
        ("900777333-8", "Energía Fusion Colombia", "contacto@fusion.example", [6, 9], []),
        ("900222999-9", "NanoMateriales del Sur", "ventas@nanomateriales.example", [5, 11], []),
        ("900333111-0", "DataMining Latinoamérica", "bigdata@datamining.example", [2, 11],
            [("Sector", "Tecnología")]),
        ("811000555-1", "AstroConsulting Group", "contact@astroconsulting.example", [3, 4, 8], []),
        ("20456789012", "Quantum Bytes Perú", "info@quantumperu.example", [0, 2, 10], []),
    ];

    private readonly TekusDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DevelopmentSeeder> _logger;

    public DevelopmentSeeder(
        TekusDbContext db,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        ILogger<DevelopmentSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>Seeds whatever is missing. Safe to run repeatedly (idempotent).</summary>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.UtcNow();
        var changed = false;

        changed |= await SeedUsersAsync(now, cancellationToken);
        changed |= await SeedCatalogueAsync(now, cancellationToken);

        if (changed)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return changed;
    }

    private async Task<bool> SeedUsersAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(cancellationToken))
        {
            return false;
        }

        _db.Users.AddRange(
            User.Create("admin@tekus.co", "Tekus Administrator", _passwordHasher.Hash(DemoPassword), UserRole.Admin, now),
            User.Create("analyst@tekus.co", "Tekus Analyst", _passwordHasher.Hash(DemoPassword), UserRole.Analyst, now));

        return true;
    }

    private async Task<bool> SeedCatalogueAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (await _db.Services.AnyAsync(cancellationToken))
        {
            return false;
        }

        // Countries first: ServiceCountries has a foreign key to this snapshot.
        var countries = SeedCountries
            .Select(seed => Country.Create(seed.Code, seed.Name, seed.Region, seed.Currency, seed.Alpha3, now))
            .ToArray();

        await _db.Countries.AddRangeAsync(countries, cancellationToken);

        var services = new List<Service>(SeedServices.Length);

        foreach (var seed in SeedServices)
        {
            var service = Service.Create(seed.Name, seed.Rate, seed.Description, now);

            foreach (var countryCode in seed.Countries)
            {
                service.OfferIn(countryCode, now);
            }

            services.Add(service);
        }

        await _db.Services.AddRangeAsync(services, cancellationToken);

        var providers = new List<Provider>(SeedProviders.Length);

        foreach (var seed in SeedProviders)
        {
            var provider = Provider.Create(seed.Nit, seed.Name, seed.Email, now);

            foreach (var (key, value) in seed.Fields)
            {
                provider.UpsertCustomField(key, value, now);
            }

            foreach (var serviceIndex in seed.Services)
            {
                provider.AssignService(services[serviceIndex].Id, now);
            }

            providers.Add(provider);
        }

        await _db.Providers.AddRangeAsync(providers, cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Development seed created: {Countries} countries, {Services} services, {Providers} providers",
                countries.Length,
                services.Count,
                providers.Count);
        }

        return true;
    }
}
