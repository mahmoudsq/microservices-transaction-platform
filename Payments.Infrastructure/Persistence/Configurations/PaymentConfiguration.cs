using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : BaseEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Payments");

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.Currency).HasMaxLength(3);

        builder.Property(x => x.Status).HasConversion<int>();

        builder.Property(x => x.CorrelationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.TransactionId).IsUnique();

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
