using MediatR;

namespace Transactions.Application.Commands.CompleteTransaction;

public record CompleteTransactionCommand(Guid TransactionId) : IRequest;
