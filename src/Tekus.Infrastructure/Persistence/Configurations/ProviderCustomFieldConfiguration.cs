using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Domain.Providers;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class ProviderCustomFieldConfiguration : IEntityTypeConfiguration<ProviderCustomField>
{
    public void Configure(EntityTypeBuilder<ProviderCustomField> builder)
    {
        builder.ToTable("ProviderCustomFields");
        builder.HasKey(field => field.Id);
        builder.Property(field => field.Id).ValueGeneratedNever();

        builder.Property(field => field.Key).HasMaxLength(ProviderCustomField.KeyMaxLength).IsRequired();
        builder.Property(field => field.Value).HasMaxLength(ProviderCustomField.ValueMaxLength).IsRequired();
        builder.Property(field => field.CreatedAtUtc).IsRequired();
        builder.Property(field => field.UpdatedAtUtc).IsRequired();

        // A field name is unique within a provider (the DB collation makes it case-insensitive).
        builder.HasIndex(field => new { field.ProviderId, field.Key }).IsUnique();
    }
}


