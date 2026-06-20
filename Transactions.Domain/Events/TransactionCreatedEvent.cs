using BuildingBlocks.Abstractions;

namespace Transactions.Domain.Events;

public class TransactionSubmittedEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public Guid TransactionId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = default!;
    public string CorrelationId { get; init; } = default!;
}
