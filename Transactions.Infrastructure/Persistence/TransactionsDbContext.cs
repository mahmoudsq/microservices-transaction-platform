using BuildingBlocks.Abstractions;
using BuildingBlocks.Correlation;
using BuildingBlocks.Idempotency;
using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Persistence;

public class TransactionsDbContext(
    DbContextOptions<TransactionsDbContext> options,
    ICorrelationContext correlationContext)
    : DbContext(options), IApplicationDbContext
{
    public string CorrelationId => correlationContext.CorrelationId;

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);
    }
}
