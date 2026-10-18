using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tekus.Infrastructure.Persistence.Auditing;

namespace Tekus.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.OccurredAtUtc).IsRequired();
        builder.Property(entry => entry.EventType).HasMaxLength(128).IsRequired();
        builder.Property(entry => entry.AggregateType).HasMaxLength(64).IsRequired();
        builder.Property(entry => entry.AggregateId).IsRequired();
        builder.Property(entry => entry.Summary).HasMaxLength(1000).IsRequired();

        builder.HasIndex(entry => entry.OccurredAtUtc);
        builder.HasIndex(entry => new { entry.AggregateType, entry.AggregateId });
    }
}
