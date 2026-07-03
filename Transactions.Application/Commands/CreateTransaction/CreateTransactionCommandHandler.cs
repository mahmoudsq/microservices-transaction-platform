using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Domain.Abstractions;
using Transactions.Application.DTOs;
using Transactions.Domain.Entities;

namespace Transactions.Application.Commands.CreateTransaction;

public class CreateTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null)
            return existing.ToDto();

        var transaction = new Transaction(
            request.Reference,
            request.Currency,
            request.IdempotencyKey);

        await repository.AddAsync(transaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToDto();
    }
}
