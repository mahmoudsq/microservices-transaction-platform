using BuildingBlocks.Abstractions;

namespace Payments.Domain.Events;

public class PaymentFailedEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid PaymentId { get; init; }
    public Guid TransactionId { get; init; }
}
