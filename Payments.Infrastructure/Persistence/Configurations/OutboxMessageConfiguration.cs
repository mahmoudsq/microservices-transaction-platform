using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payments.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : BaseEntityConfiguration<OutboxMessage>
{
    public override void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        base.Configure(builder);
        builder.ToTable("OutboxMessages");

        builder.Property(x => x.Payload).IsRequired();

        builder.Property(x => x.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(x => new { x.ProcessedAt, x.LockedUntil, x.OccurredAt });
    }
}
