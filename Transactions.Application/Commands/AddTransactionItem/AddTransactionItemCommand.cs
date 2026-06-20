using MediatR;
using Transactions.Application.DTOs;

namespace Transactions.Application.Commands.AddTransactionItem;

public record AddTransactionItemCommand(
    Guid TransactionId,
    string ProductId,
    int Quantity,
    decimal Price) : IRequest<TransactionDto>;
