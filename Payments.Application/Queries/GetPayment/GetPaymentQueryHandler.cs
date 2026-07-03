using MediatR;
using Payments.Domain.Abstractions;
using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetPayment;

public class GetPaymentQueryHandler(IPaymentRepository repository)
    : IRequestHandler<GetPaymentQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await repository.GetByIdAsync(request.Id, cancellationToken);
        return payment?.ToDto();
    }
}
