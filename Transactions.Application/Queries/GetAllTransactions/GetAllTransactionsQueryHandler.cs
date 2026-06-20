using MediatR;
using Transactions.Application.Abstractions;
using Transactions.Application.DTOs;

namespace Transactions.Application.Queries.GetAllTransactions;

public class GetAllTransactionsQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetAllTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<IReadOnlyList<TransactionDto>> Handle(
        GetAllTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await repository.GetAllAsync(cancellationToken);
        return [.. transactions.Select(t => t.ToDto())];
    }
}
