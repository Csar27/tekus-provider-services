using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Providers;
using Tekus.Domain.Services;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderService>
{
    public void Configure(EntityTypeBuilder<ProviderService> builder)
    {
        builder.ToTable("ProviderServices");
        builder.HasKey(offering => offering.Id);

        builder.Property(offering => offering.AssignedAtUtc).IsRequired();

        // A provider cannot offer the same service twice.
        builder.HasIndex(offering => new { offering.ProviderId, offering.ServiceId }).IsUnique();

        // Cross-aggregate integrity: the referenced service must exist in the catalogue.
        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(offering => offering.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
