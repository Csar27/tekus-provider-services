using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Services;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(service => service.Id);

        builder.Property(service => service.Name).HasMaxLength(Service.NameMaxLength).IsRequired();
        builder.Property(service => service.HourlyRateUsd).HasPrecision(18, Service.RateDecimals).IsRequired();
        builder.Property(service => service.Description).HasMaxLength(Service.DescriptionMaxLength);
        builder.Property(service => service.IsActive).IsRequired();
        builder.Property(service => service.CreatedAtUtc).IsRequired();
        builder.Property(service => service.UpdatedAtUtc);

        builder.Property(service => service.Version).IsConcurrencyToken();

        builder.HasIndex(service => service.Name).IsUnique();
        builder.HasIndex(service => service.IsActive);

        builder.Ignore(service => service.DomainEvents);

        builder.HasMany(service => service.Countries)
            .WithOne()
            .HasForeignKey(country => country.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(service => service.Countries).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
