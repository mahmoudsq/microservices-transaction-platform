using BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Idempotency;

public class IdempotencyService(IApplicationDbContext context) : IIdempotencyService
{
    private readonly IApplicationDbContext _context = context;

    public async Task<bool> ExistsAsync(
        Guid messageId, string consumer,
        CancellationToken ct = default)
    {
        return await _context.InboxMessages
            .AnyAsync(x => x.Id == messageId && x.Consumer == consumer, ct);
    }

    public async Task MarkProcessedAsync(
        Guid messageId, string consumer,
        CancellationToken ct = default)
    {
        await _context.InboxMessages.AddAsync(new InboxMessage(messageId)
        {
            Consumer = consumer,
            ProcessedAt = DateTime.UtcNow,
        }, ct);
        await _context.SaveChangesAsync(ct);
    }
}
