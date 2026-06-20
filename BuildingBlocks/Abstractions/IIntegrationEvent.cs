namespace BuildingBlocks.Abstractions;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string CorrelationId { get; }
}
