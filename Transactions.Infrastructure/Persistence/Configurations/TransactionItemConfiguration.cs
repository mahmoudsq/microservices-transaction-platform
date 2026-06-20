using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Persistence.Configurations;

public sealed class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable("TransactionItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Ignore(x => x.Subtotal);

        builder.HasIndex(x => x.TransactionId);
    }
}
