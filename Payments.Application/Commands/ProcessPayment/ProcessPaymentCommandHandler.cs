using BuildingBlocks.Abstractions;
using MediatR;
using Payments.Application.Abstractions;
using Payments.Application.DTOs;
using Payments.Domain.Entities;

namespace Payments.Application.Commands.StartPayment;

public class StartPaymentCommandHandler(
    IPaymentRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StartPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(
        StartPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);
        if (existing is not null)
            return existing.ToDto();

        var payment = new Payment(
            request.TransactionId,
            request.Amount,
            request.Currency,
            request.CorrelationId);

        await repository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }
}
