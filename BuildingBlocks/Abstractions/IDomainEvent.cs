namespace BuildingBlocks.Abstractions;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}