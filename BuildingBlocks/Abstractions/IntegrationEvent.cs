namespace BuildingBlocks.Abstractions;

public class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = default!;
}
