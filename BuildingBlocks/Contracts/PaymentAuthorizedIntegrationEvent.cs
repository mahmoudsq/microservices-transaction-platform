using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Contracts;

public class PaymentConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public Guid TransactionId { get; init; }
}
