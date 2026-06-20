using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Application.Abstractions;

namespace Transactions.Application.Commands.CancelTransaction;

public class CancelTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelTransactionCommand>
{
    public async Task Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        transaction.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
