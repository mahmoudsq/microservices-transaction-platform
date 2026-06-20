using BuildingBlocks.Abstractions;
using MediatR;
using Transactions.Application.Abstractions;
using Transactions.Application.DTOs;

namespace Transactions.Application.Commands.AddTransactionItem;

public class AddTransactionItemCommandHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddTransactionItemCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(
        AddTransactionItemCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await repository.GetByIdAsync(request.TransactionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found.");

        transaction.AddItem(request.ProductId, request.Quantity, request.Price);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToDto();
    }
}
