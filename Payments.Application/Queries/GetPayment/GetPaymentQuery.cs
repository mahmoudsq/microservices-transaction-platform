using MediatR;
using Payments.Application.DTOs;

namespace Payments.Application.Queries.GetPayment;

public record GetPaymentQuery(Guid Id) : IRequest<PaymentDto?>;
