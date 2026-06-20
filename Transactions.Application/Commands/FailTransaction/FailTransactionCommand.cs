using MediatR;

namespace Transactions.Application.Commands.FailTransaction;

public record FailTransactionCommand(Guid TransactionId) : IRequest;
