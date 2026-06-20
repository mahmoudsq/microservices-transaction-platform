using BuildingBlocks.SharedEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payments.Infrastructure.Persistence.Configurations;

public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : class
{
    public static readonly string CreatedAt = "CreatedAt";
    public static readonly string UpdatedAt = "UpdatedAt";

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey("Id");

        if (typeof(IAudiEntity).IsAssignableFrom(typeof(T)))
        {
            builder.Property<DateTime>(CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property<DateTime>(UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAddOrUpdate();
        }
    }
}
