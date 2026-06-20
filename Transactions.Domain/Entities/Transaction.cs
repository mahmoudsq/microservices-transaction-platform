using BuildingBlocks.Exceptions;
using BuildingBlocks.SharedEntities;
using Transactions.Domain.Enums;
using Transactions.Domain.Events;

namespace Transactions.Domain.Entities;

public class Transaction : Entity, IAudiEntity
{
    public string Reference { get; private set; } = default!;
    public string Currency { get; private set; } = "USD";
    public TransactionStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;

    private readonly List<TransactionItem> _items = [];
    public IReadOnlyCollection<TransactionItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.Subtotal);

    private Transaction() { }

    public Transaction(string reference, string currency, string idempotencyKey)
    {
        SetId();
        Reference = reference;
        Currency = currency;
        IdempotencyKey = idempotencyKey;
        Status = TransactionStatus.Draft;
    }

    public void AddItem(string productId, int quantity, decimal price)
    {
        if (Status != TransactionStatus.Draft)
            throw new DomainException("Transaction.CannotModifyAfterSubmission");

        _items.Add(new TransactionItem(Id, productId, quantity, price));
    }

    public void Submit(string correlationId)
    {
        if (Status != TransactionStatus.Draft)
            throw new DomainException("Transaction.AlreadySubmitted");
        if (!_items.Any())
            throw new DomainException("Transaction.CannotSubmitWithoutItems");
        if (TotalAmount <= 0)
            throw new DomainException("Transaction.TotalAmountMustBePositive");

        Status = TransactionStatus.Submitted;
        AddDomainEvent(new TransactionSubmittedEvent
        {
            TransactionId = Id,
            TotalAmount = TotalAmount,
            Currency = Currency,
            CorrelationId = correlationId
        });
    }

    public void Cancel()
    {
        if (Status == TransactionStatus.Completed)
            throw new DomainException("Transaction.CannotCancelCompleted");
        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Transaction.AlreadyCancelled");
        if (Status == TransactionStatus.Failed)
            throw new DomainException("Transaction.CannotCancelFailed");

        Status = TransactionStatus.Cancelled;
        AddDomainEvent(new TransactionCancelledEvent { TransactionId = Id });
    }

    public void Complete()
    {
        if (Status != TransactionStatus.Submitted)
            throw new DomainException("Transaction.CannotCompleteFromCurrentState");

        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new TransactionCompletedEvent { TransactionId = Id });
    }

    public void Fail()
    {
        if (Status == TransactionStatus.Completed)
            throw new DomainException("Transaction.CannotFailCompleted");
        if (Status == TransactionStatus.Failed)
            throw new DomainException("Transaction.AlreadyFailed");
        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Transaction.CannotFailCancelled");

        Status = TransactionStatus.Failed;
        AddDomainEvent(new TransactionFailedEvent { TransactionId = Id });
    }
}
