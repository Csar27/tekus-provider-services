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

        // The domain assigns the identifier itself, exactly like every other entity in the
        // model. Without this EF treats Id as store-generated, so a freshly created join row
        // is attached as "Modified" instead of "Added" and SaveChanges issues an UPDATE against
        // a row that does not exist yet (DbUpdateConcurrencyException on PUT .../services).
        builder.Property(offering => offering.Id).ValueGeneratedNever();

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
