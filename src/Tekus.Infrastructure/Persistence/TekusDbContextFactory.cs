using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tekus.Infrastructure.Persistence;

/// <summary>
/// Factory used by the EF Core command line tools (<c>dotnet ef migrations add ...</c>) when they
/// need to create the context outside of the running API. The connection string only has to be
/// parseable here: migrations are generated from the model, not from a live database.
/// </summary>
internal sealed class TekusDbContextFactory : IDesignTimeDbContextFactory<TekusDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=TekusProviderServices;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public TekusDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TekusDbContext>()
            .UseSqlServer(DesignTimeConnectionString)
            .Options;

        return new TekusDbContext(options);
    }
}
