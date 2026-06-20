namespace BuildingBlocks.Idempotency;

public interface IIdempotencyService
{
    Task<bool> ExistsAsync(Guid messageId, string consumer, CancellationToken ct = default);
    Task MarkProcessedAsync(Guid messageId, string consumer, CancellationToken ct = default);
}
