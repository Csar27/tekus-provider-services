using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Countries;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        // Natural key: the ISO alpha-2 code is the primary key, so no surrogate id is needed and
        // the join from ServiceCountries is a single column.
        builder.HasKey(country => country.Code);
        builder.Property(country => country.Code)
            .HasMaxLength(CountryCode.Length)
            .HasConversion(code => code.Value, value => CountryCode.Create(value))
            .IsRequired();

        builder.Property(country => country.Name).HasMaxLength(120).IsRequired();
        builder.Property(country => country.Region).HasMaxLength(80).IsRequired();
        builder.Property(country => country.CurrencyCode).HasMaxLength(3);
        builder.Property(country => country.Alpha3Code).HasMaxLength(3);
        builder.Property(country => country.LastSyncedAtUtc).IsRequired();

        builder.HasIndex(country => country.Name);
        builder.HasIndex(country => country.Region);
    }
}
