using Microsoft.EntityFrameworkCore;
using Tekus.Domain.Countries;
using Tekus.Domain.Providers;
using Tekus.Domain.Services;
using Tekus.Domain.Users;
using Tekus.Infrastructure.Persistence.Auditing;

namespace Tekus.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of persistence for the whole model. Entity configurations live in dedicated
/// <see cref="IEntityTypeConfiguration{TEntity}"/> classes (one per table) and are discovered by
/// assembly scan, which keeps this class tiny and the mapping readable.
/// </summary>
public sealed class TekusDbContext : DbContext
{
    public TekusDbContext(DbContextOptions<TekusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<ProviderCustomField> ProviderCustomFields => Set<ProviderCustomField>();

    public DbSet<ProviderService> ProviderServices => Set<ProviderService>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<ServiceCountry> ServiceCountries => Set<ServiceCountry>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TekusDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
