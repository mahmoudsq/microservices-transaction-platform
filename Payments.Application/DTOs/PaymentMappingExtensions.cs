using Payments.Domain.Entities;

namespace Payments.Application.DTOs;

public static class PaymentMappingExtensions
{
    public static PaymentDto ToDto(this Payment p) =>
        new(p.Id, p.TransactionId, p.Amount, p.Currency, p.Status.ToString(),
            p.ConfirmedAt, p.CorrelationId);
}
