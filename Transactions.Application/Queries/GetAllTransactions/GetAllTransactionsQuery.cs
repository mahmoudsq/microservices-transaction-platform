using MediatR;
using Transactions.Application.DTOs;

namespace Transactions.Application.Queries.GetAllTransactions;

public record GetAllTransactionsQuery : IRequest<IReadOnlyList<TransactionDto>>;
