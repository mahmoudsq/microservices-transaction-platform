using FluentAssertions;
using NSubstitute;
using Payments.Domain.Abstractions;
using Payments.Application.Queries.GetPayment;
using Payments.Domain.Entities;

namespace Payments.Tests.Application.Queries;

public class GetPaymentQueryHandlerTests
{
    private readonly IPaymentRepository _repository = Substitute.For<IPaymentRepository>();
    private readonly GetPaymentQueryHandler _handler;

    public GetPaymentQueryHandlerTests()
    {
        _handler = new GetPaymentQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_ReturnsDto()
    {
        var transactionId = Guid.NewGuid();
        var payment = new Payment(transactionId, 200m, "USD", "corr");
        _repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var result = await _handler.Handle(new GetPaymentQuery(payment.Id), default);

        result.Should().NotBeNull();
        result!.TransactionId.Should().Be(transactionId);
        result.Amount.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _handler.Handle(new GetPaymentQuery(id), default);

        result.Should().BeNull();
    }
}
