using BuildingBlocks.Exceptions;
using BuildingBlocks.SharedEntities;
using Payments.Domain.Enums;
using Payments.Domain.Events;

namespace Payments.Domain.Entities;

public class Payment : Entity, IAudiEntity
{
    public Guid TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public string CorrelationId { get; private set; } = default!;

    private Payment() { }

    public Payment(Guid transactionId, decimal amount, string currency, string correlationId)
    {
        SetId();
        TransactionId = transactionId;
        Amount = amount;
        Currency = currency;
        CorrelationId = correlationId;
        Status = PaymentStatus.Pending;
        AddDomainEvent(new PaymentStartedEvent { PaymentId = Id, TransactionId = TransactionId });
    }

    public void Confirm()
    {
        if (Status == PaymentStatus.Confirmed)
            throw new DomainException("Payment.AlreadyConfirmed");
        if (Status == PaymentStatus.Failed)
            throw new DomainException("Payment.CannotConfirmFailed");

        Status = PaymentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        AddDomainEvent(new PaymentConfirmedEvent { PaymentId = Id, TransactionId = TransactionId });
    }

    public void Fail()
    {
        if (Status == PaymentStatus.Confirmed)
            throw new DomainException("Payment.CannotFailConfirmed");
        if (Status == PaymentStatus.Failed)
            throw new DomainException("Payment.AlreadyFailed");

        Status = PaymentStatus.Failed;
        AddDomainEvent(new PaymentFailedEvent { PaymentId = Id, TransactionId = TransactionId });
    }
}
