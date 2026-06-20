using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Transactions.Application.Abstractions;
using Transactions.Application.Commands.FailTransaction;
using Transactions.Domain.Entities;
using Transactions.Domain.Enums;

namespace Transactions.Tests.Application.Commands;

public class FailTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly FailTransactionCommandHandler _handler;

    public FailTransactionCommandHandlerTests()
    {
        _handler = new FailTransactionCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_CallsFailAndSaves()
    {
        var transaction = new Transaction("REF", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        await _handler.Handle(new FailTransactionCommand(transaction.Id), default);

        transaction.Status.Should().Be(TransactionStatus.Failed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = async () => await _handler.Handle(new FailTransactionCommand(id), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainException_Propagates()
    {
        var transaction = new Transaction("REF", "USD", "key");
        transaction.Fail();
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var act = async () => await _handler.Handle(new FailTransactionCommand(transaction.Id), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Transaction.AlreadyFailed");
    }
}
