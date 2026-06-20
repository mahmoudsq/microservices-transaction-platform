namespace Transactions.Application.DTOs;

public record TransactionItemDto(
    string ProductId,
    int Quantity,
    decimal Price,
    decimal Subtotal);

public record TransactionDto(
    Guid Id,
    string Reference,
    decimal TotalAmount,
    string Currency,
    string Status,
    DateTime? CompletedAt,
    string IdempotencyKey,
    IReadOnlyCollection<TransactionItemDto> Items);
