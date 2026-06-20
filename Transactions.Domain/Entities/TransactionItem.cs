using BuildingBlocks.Exceptions;
using BuildingBlocks.SharedEntities;

namespace Transactions.Domain.Entities;

public class TransactionItem : Entity
{
    public Guid TransactionId { get; private set; }
    public string ProductId { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal Subtotal => Quantity * Price;

    private TransactionItem() { }

    public TransactionItem(Guid transactionId, string productId, int quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("TransactionItem.ProductId.Required");
        if (quantity <= 0)
            throw new DomainException("TransactionItem.Quantity.MustBePositive");
        if (price <= 0)
            throw new DomainException("TransactionItem.Price.MustBePositive");

        SetId();
        TransactionId = transactionId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
}
