using BuildingBlocks.Idempotency;
using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Abstractions;

public interface IApplicationDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
