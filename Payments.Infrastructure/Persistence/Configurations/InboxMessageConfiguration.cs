using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payments.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : BaseEntityConfiguration<InboxMessage>
{
    public override void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("InboxMessages");

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Consumer)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ReceivedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.Id, x.Consumer })
            .IsUnique();

        builder.HasIndex(x => x.ProcessedAt);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
