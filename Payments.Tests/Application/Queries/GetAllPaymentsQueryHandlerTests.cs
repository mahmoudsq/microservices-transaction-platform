using FluentAssertions;
using NSubstitute;
using Payments.Domain.Abstractions;
using Payments.Application.Queries.GetAllPayments;
using Payments.Domain.Entities;

namespace Payments.Tests.Application.Queries;

public class GetAllPaymentsQueryHandlerTests
{
    private readonly IPaymentRepository _repository = Substitute.For<IPaymentRepository>();
    private readonly GetAllPaymentsQueryHandler _handler;

    public GetAllPaymentsQueryHandlerTests()
    {
        _handler = new GetAllPaymentsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenPaymentsExist_ReturnsMappedDtos()
    {
        var payments = new List<Payment>
        {
            new(Guid.NewGuid(), 100m, "USD", "corr-1"),
            new(Guid.NewGuid(), 200m, "EUR", "corr-2")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(payments);

        var result = await _handler.Handle(new GetAllPaymentsQuery(), default);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoPayments_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Payment>());

        var result = await _handler.Handle(new GetAllPaymentsQuery(), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsDtoCountMatchingRepository()
    {
        var payments = Enumerable.Range(1, 4)
            .Select(i => new Payment(Guid.NewGuid(), i * 50m, "USD", $"corr-{i}"))
            .ToList();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(payments);

        var result = await _handler.Handle(new GetAllPaymentsQuery(), default);

        result.Should().HaveCount(4);
    }
}
