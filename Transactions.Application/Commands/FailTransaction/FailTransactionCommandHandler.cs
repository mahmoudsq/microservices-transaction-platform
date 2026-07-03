using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Domain.Abstractions;

namespace Transactions.Application.Commands.FailTransaction;

public class FailTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<FailTransactionCommand>
{
    public async Task Handle(FailTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        transaction.Fail();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
