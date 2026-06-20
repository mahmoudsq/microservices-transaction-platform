using MediatR;
using Transactions.Application.DTOs;

namespace Transactions.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    string Reference,
    string Currency,
    string IdempotencyKey) : IRequest<TransactionDto>;
