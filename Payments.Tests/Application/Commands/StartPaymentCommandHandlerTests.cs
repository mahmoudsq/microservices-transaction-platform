using BuildingBlocks.Abstractions;
using FluentAssertions;
using NSubstitute;
using Payments.Application.Abstractions;
using Payments.Application.Commands.StartPayment;
using Payments.Domain.Entities;

namespace Payments.Tests.Application.Commands;

public class StartPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _repository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StartPaymentCommandHandler _handler;

    public StartPaymentCommandHandlerTests()
    {
        _handler = new StartPaymentCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTransactionAlreadyHasPayment_ReturnsExistingPayment_WithoutSaving()
    {
        var transactionId = Guid.NewGuid();
        var existing = new Payment(transactionId, 100m, "USD", "corr");
        _repository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.Handle(
            new StartPaymentCommand(transactionId, 100m, "USD", "corr"), default);

        result.Id.Should().Be(existing.Id);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewTransaction_CreatesPayment_AndSaves()
    {
        var transactionId = Guid.NewGuid();
        _repository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        await _handler.Handle(new StartPaymentCommand(transactionId, 200m, "EUR", "corr-1"), default);

        await _repository.Received(1).AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewTransaction_ReturnsPaymentDto()
    {
        var transactionId = Guid.NewGuid();
        _repository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var result = await _handler.Handle(
            new StartPaymentCommand(transactionId, 150m, "USD", "corr"), default);

        result.Should().NotBeNull();
        result.TransactionId.Should().Be(transactionId);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_WhenNewTransaction_CallsAddAsync()
    {
        var transactionId = Guid.NewGuid();
        _repository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>()).Returns((Payment?)null);

        await _handler.Handle(new StartPaymentCommand(transactionId, 75m, "USD", "corr"), default);

        await _repository.Received(1).AddAsync(
            Arg.Is<Payment>(p => p.TransactionId == transactionId && p.Amount == 75m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewTransaction_CallsSaveChangesAsync()
    {
        _repository.GetByTransactionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        await _handler.Handle(
            new StartPaymentCommand(Guid.NewGuid(), 100m, "USD", "corr"), default);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
