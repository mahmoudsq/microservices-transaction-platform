using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Contracts;

public class TransactionSubmittedIntegrationEvent : IntegrationEvent
{
    public Guid TransactionId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = default!;
}
