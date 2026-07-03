using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : BaseEntityConfiguration<Transaction>
{
    public override void Configure(EntityTypeBuilder<Transaction> builder)
    {
        base.Configure(builder);

        builder.ToTable("Transactions");

        builder.Property(x => x.Reference)
           .IsRequired()
           .HasMaxLength(100);

        builder.HasIndex(x => x.Reference)
            .IsUnique();

        builder.Ignore(x => x.TotalAmount);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(i => i.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();

        builder.Property<byte[]>("RowVersion")
           .IsRowVersion();
    }
}
