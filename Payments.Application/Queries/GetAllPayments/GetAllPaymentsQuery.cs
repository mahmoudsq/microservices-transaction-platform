using MediatR;
using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetAllPayments;

public record GetAllPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;
