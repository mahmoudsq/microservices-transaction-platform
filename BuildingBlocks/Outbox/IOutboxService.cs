namespace BuildingBlocks.Outbox;

public interface IOutboxService
{
    Task AddAsync(OutboxMessage message, CancellationToken ct = default);
}
