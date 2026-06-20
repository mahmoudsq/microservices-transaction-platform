using MediatR;
using Payments.Application.DTOs;

namespace Payments.Application.Commands.StartPayment;

public record StartPaymentCommand(
    Guid TransactionId,
    decimal Amount,
    string Currency,
    string CorrelationId) : IRequest<PaymentDto>;
