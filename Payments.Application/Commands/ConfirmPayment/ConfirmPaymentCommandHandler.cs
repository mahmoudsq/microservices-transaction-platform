using BuildingBlocks.Abstractions;
using MediatR;
using Payments.Application.Abstractions;

namespace Payments.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler(
    IPaymentRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmPaymentCommand>
{
    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payment {request.PaymentId} not found.");

        payment.Confirm();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
