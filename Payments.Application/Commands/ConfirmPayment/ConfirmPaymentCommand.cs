using MediatR;

namespace Payments.Application.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(Guid PaymentId) : IRequest;
