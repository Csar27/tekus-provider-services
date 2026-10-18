using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tekus.Infrastructure.Persistence.Seeding;

/// <summary>
/// Prepares the database when the application starts: applies the pending EF Core migrations and
/// (optionally) runs the development seed so a fresh checkout is usable immediately.
/// </summary>
internal sealed class DatabaseBootstrapper
{
    private readonly TekusDbContext _db;
    private readonly DevelopmentSeeder _seeder;
    private readonly ILogger<DatabaseBootstrapper> _logger;

    public DatabaseBootstrapper(TekusDbContext db, DevelopmentSeeder seeder, ILogger<DatabaseBootstrapper> logger)
    {
        _db = db;
        _seeder = seeder;
        _logger = logger;
    }

    /// <param name="seedDevelopmentData">
    /// When <c>true</c> the deterministic demo data (users, countries, services and providers) is
    /// inserted if the tables are empty. It is enabled only in the Development environment.
    /// </param>
    public async Task InitializeAsync(bool seedDevelopmentData, CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsRelational())
        {
            var pendingMigrations = (await _db.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();

            if (pendingMigrations.Length > 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Applying {Count} pending database migrations", pendingMigrations.Length);
                }
                await _db.Database.MigrateAsync(cancellationToken);
            }
        }
        else
        {
            // Non relational providers (e.g. the in-memory database used by some test hosts).
            await _db.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (seedDevelopmentData && await _seeder.SeedAsync(cancellationToken))
        {
            _logger.LogInformation("Development seed applied");
        }
    }
}
