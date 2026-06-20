using BuildingBlocks.SharedEntities;

namespace BuildingBlocks.Outbox;

public class OutboxMessage : Entity
{
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public string? CorrelationId { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public DateTime? LockedUntil { get; set; }
    public int RetryCount { get; set; }

    public OutboxMessage() => SetId();
}
