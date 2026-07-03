using BuildingBlocks.Exceptions;
using MediatR;
using Transactions.Domain.Abstractions;
using Transactions.Application.DTOs;

namespace Transactions.Application.Queries.GetTransaction;

public class GetTransactionQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetTransactionQuery, TransactionDto?>
{
    public async Task<TransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Transaction.NotFound");
        return transaction?.ToDto();
    }
}
