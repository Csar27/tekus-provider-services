using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Countries;
using Tekus.Domain.Services;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class ServiceCountryConfiguration : IEntityTypeConfiguration<ServiceCountry>
{
    public void Configure(EntityTypeBuilder<ServiceCountry> builder)
    {
        builder.ToTable("ServiceCountries");
        builder.HasKey(country => country.Id);
        builder.Property(country => country.Id).ValueGeneratedNever();

        builder.Property(country => country.CountryCode)
            .HasMaxLength(CountryCode.Length)
            .HasConversion(code => code.Value, value => CountryCode.Create(value))
            .IsRequired();

        builder.Property(country => country.AvailableSinceUtc).IsRequired();

        builder.HasIndex(country => new { country.ServiceId, country.CountryCode }).IsUnique();

        // The code must exist in the local snapshot of the external catalogue. The application
        // validates it first (helpful 400); this FK is the last line of defence.
        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(country => country.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


