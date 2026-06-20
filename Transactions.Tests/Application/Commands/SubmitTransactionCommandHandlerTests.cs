using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Transactions.Application.Abstractions;
using Transactions.Application.Commands.SubmitTransaction;
using Transactions.Domain.Entities;
using Transactions.Domain.Events;

namespace Transactions.Tests.Application.Commands;

public class SubmitTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SubmitTransactionCommandHandler _handler;

    public SubmitTransactionCommandHandlerTests()
    {
        _handler = new SubmitTransactionCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_CallsSubmitAndSaves()
    {
        var transaction = new Transaction("REF", "USD", "key");
        transaction.AddItem("P1", 1, 10m);
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        await _handler.Handle(new SubmitTransactionCommand(transaction.Id, "corr-1"), default);

        transaction.Status.Should().Be(Transactions.Domain.Enums.TransactionStatus.Submitted);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = async () => await _handler.Handle(new SubmitTransactionCommand(id, "corr"), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainException_Propagates()
    {
        var transaction = new Transaction("REF", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var act = async () => await _handler.Handle(
            new SubmitTransactionCommand(transaction.Id, "corr"), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Transaction.CannotSubmitWithoutItems");
    }

    [Fact]
    public async Task Handle_PassesCorrelationIdToSubmit()
    {
        var transaction = new Transaction("REF", "USD", "key");
        transaction.AddItem("P1", 1, 10m);
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        await _handler.Handle(new SubmitTransactionCommand(transaction.Id, "corr-expected"), default);

        var evt = transaction.DomainEvents.OfType<TransactionSubmittedEvent>().Single();
        evt.CorrelationId.Should().Be("corr-expected");
    }
}
