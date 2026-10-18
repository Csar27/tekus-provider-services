using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Providers;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");
        builder.HasKey(provider => provider.Id);
        builder.Property(provider => provider.Id).ValueGeneratedNever();

        builder.Property(provider => provider.Nit).HasMaxLength(Provider.NitMaxLength).IsRequired();
        builder.Property(provider => provider.Name).HasMaxLength(Provider.NameMaxLength).IsRequired();
        builder.Property(provider => provider.Email).HasMaxLength(Provider.EmailMaxLength).IsRequired();
        builder.Property(provider => provider.IsActive).IsRequired();
        builder.Property(provider => provider.CreatedAtUtc).IsRequired();
        builder.Property(provider => provider.UpdatedAtUtc);

        // Optimistic concurrency: EF adds "WHERE Version = @original" to every UPDATE.
        builder.Property(provider => provider.Version).IsConcurrencyToken();

        builder.HasIndex(provider => provider.Nit).IsUnique();
        builder.HasIndex(provider => provider.Name);
        builder.HasIndex(provider => provider.IsActive);

        // Domain events are an in-memory concern, never a column.
        builder.Ignore(provider => provider.DomainEvents);

        builder.HasMany(provider => provider.CustomFields)
            .WithOne()
            .HasForeignKey(field => field.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(provider => provider.CustomFields).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(provider => provider.OfferedServices)
            .WithOne()
            .HasForeignKey(offering => offering.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(provider => provider.OfferedServices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}


