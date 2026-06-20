using BuildingBlocks.Abstractions;

namespace Transactions.Domain.Events;

public class TransactionCancelledEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid TransactionId { get; init; }
}
