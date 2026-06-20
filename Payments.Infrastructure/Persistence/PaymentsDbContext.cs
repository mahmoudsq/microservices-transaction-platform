using BuildingBlocks.Abstractions;
using BuildingBlocks.Correlation;
using BuildingBlocks.Idempotency;
using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext(
    DbContextOptions<PaymentsDbContext> options,
    ICorrelationContext correlationContext)
    : DbContext(options), IApplicationDbContext
{
    public string CorrelationId => correlationContext.CorrelationId;

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
    }
}
