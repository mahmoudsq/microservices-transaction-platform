using Transactions.Domain.Entities;

namespace Transactions.Application.DTOs;

public static class TransactionMappingExtensions
{
    public static TransactionDto ToDto(this Transaction t) =>
        new(t.Id, t.Reference, t.TotalAmount, t.Currency, t.Status.ToString(), t.CompletedAt, t.IdempotencyKey,
            t.Items.Select(i => new TransactionItemDto(i.ProductId, i.Quantity, i.Price, i.Subtotal))
                   .ToList().AsReadOnly());
}
