using MediatR;
using Payments.Domain.Abstractions;
using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetAllPayments;

public class GetAllPaymentsQueryHandler(IPaymentRepository repository)
    : IRequestHandler<GetAllPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetAllPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await repository.GetAllAsync(cancellationToken);
        return payments.Select(p => p.ToDto()).ToList();
    }
}
