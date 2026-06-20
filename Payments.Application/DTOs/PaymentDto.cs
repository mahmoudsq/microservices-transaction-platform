namespace Payments.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? ConfirmedAt,
    string CorrelationId);
