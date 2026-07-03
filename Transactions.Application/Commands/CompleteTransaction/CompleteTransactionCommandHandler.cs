using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Domain.Abstractions;

namespace Transactions.Application.Commands.CompleteTransaction;

public class CompleteTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteTransactionCommand>
{
    public async Task Handle(CompleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        transaction.Complete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
