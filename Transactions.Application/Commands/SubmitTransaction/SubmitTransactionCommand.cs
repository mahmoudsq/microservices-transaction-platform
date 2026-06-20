using MediatR;

namespace Transactions.Application.Commands.SubmitTransaction;

public record SubmitTransactionCommand(Guid TransactionId, string CorrelationId) : IRequest;
