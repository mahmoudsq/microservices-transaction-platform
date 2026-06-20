using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Outbox;

public class OutboxService(IApplicationDbContext context) : IOutboxService
{
    private readonly IApplicationDbContext _context = context;

    public async Task AddAsync(
        OutboxMessage message,
        CancellationToken ct = default)
    {
       
        await _context.OutboxMessages.AddAsync(message, ct);
    }
}
