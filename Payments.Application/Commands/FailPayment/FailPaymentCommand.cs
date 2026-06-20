using MediatR;

namespace Payments.Application.Commands.FailPayment;

public record FailPaymentCommand(Guid PaymentId) : IRequest;
