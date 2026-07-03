using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Payments.Domain.Abstractions;
using Payments.Application.Commands.ConfirmPayment;
using Payments.Domain.Entities;
using Payments.Domain.Enums;

namespace Payments.Tests.Application.Commands;

public class ConfirmPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _repository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConfirmPaymentCommandHandler _handler;

    public ConfirmPaymentCommandHandlerTests()
    {
        _handler = new ConfirmPaymentCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPaymentFound_CallsConfirmAndSaves()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        _repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        await _handler.Handle(new ConfirmPaymentCommand(payment.Id), default);

        payment.Status.Should().Be(PaymentStatus.Confirmed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var act = async () => await _handler.Handle(new ConfirmPaymentCommand(id), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainException_Propagates()
    {
        var payment = new Payment(Guid.NewGuid(), 100m, "USD", "corr");
        payment.Confirm();
        _repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var act = async () => await _handler.Handle(new ConfirmPaymentCommand(payment.Id), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Payment.AlreadyConfirmed");
    }
}
