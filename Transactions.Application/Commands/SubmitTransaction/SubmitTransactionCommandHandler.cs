using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Domain.Abstractions;

namespace Transactions.Application.Commands.SubmitTransaction;

public class SubmitTransactionCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitTransactionCommand>
{
    public async Task Handle(SubmitTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        transaction.Submit(request.CorrelationId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
