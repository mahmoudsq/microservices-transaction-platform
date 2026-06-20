using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Transactions.Application.Abstractions;
using Transactions.Application.Commands.CompleteTransaction;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;

namespace Transactions.Tests.Application.Commands;

public class CompleteTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CompleteTransactionCommandHandler _handler;

    public CompleteTransactionCommandHandlerTests()
    {
        _handler = new CompleteTransactionCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_CallsCompleteAndSaves()
    {
        var transaction = CreateSubmitted();
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        await _handler.Handle(new CompleteTransactionCommand(transaction.Id), default);

        transaction.Status.Should().Be(TransactionStatus.Completed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = async () => await _handler.Handle(new CompleteTransactionCommand(id), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainException_Propagates()
    {
        var transaction = new Transaction("REF", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var act = async () => await _handler.Handle(
            new CompleteTransactionCommand(transaction.Id), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Transaction.CannotCompleteFromCurrentState");
    }

    private static Transaction CreateSubmitted()
    {
        var t = new Transaction("REF", "USD", Guid.NewGuid().ToString());
        t.AddItem("P1", 1, 10m);
        t.Submit("corr");
        return t;
    }
}
