using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Contracts;

public class PaymentFailedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public Guid TransactionId { get; init; }
}
