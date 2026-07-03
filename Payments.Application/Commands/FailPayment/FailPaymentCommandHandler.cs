using BuildingBlocks.Abstractions;
using MediatR;
using Payments.Domain.Abstractions;

namespace Payments.Application.Commands.FailPayment;

public class FailPaymentCommandHandler(
    IPaymentRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<FailPaymentCommand>
{
    public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payment {request.PaymentId} not found.");

        payment.Fail();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
