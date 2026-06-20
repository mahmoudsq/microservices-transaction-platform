using MediatR;

namespace Transactions.Application.Commands.CancelTransaction;

public record CancelTransactionCommand(Guid TransactionId) : IRequest;
