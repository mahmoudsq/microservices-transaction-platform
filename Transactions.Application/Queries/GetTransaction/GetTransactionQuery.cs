using MediatR;
using Transactions.Application.DTOs;

namespace Transactions.Application.Queries.GetTransaction;

public record GetTransactionQuery(Guid Id) : IRequest<TransactionDto?>;
