using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Payments.Domain.Abstractions;
using Payments.Application.Commands.FailPayment;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Tests.Application.Commands;

public class FailPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _repository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FailPaymentCommandHandler _handler;

    public FailPaymentCommandHandlerTests()
    {
        _handler = new FailPaymentCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_CallsFailAndSaves()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        _repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        await _handler.Handle(new FailPaymentCommand(payment.Id), default);

        payment.Status.Should().Be(PaymentStatus.Failed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var act = async () => await _handler.Handle(new FailPaymentCommand(id), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainException_Propagates()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        payment.Fail();
        _repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var act = async () => await _handler.Handle(new FailPaymentCommand(payment.Id), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Payment.AlreadyFailed");
    }
}
