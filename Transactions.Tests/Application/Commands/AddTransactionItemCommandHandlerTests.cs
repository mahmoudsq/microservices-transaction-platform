using BuildingBlocks.Abstractions;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Transactions.Domain.Abstractions;
using Transactions.Application.Commands.AddTransactionItem;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Application.Commands;

public class AddTransactionItemCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AddTransactionItemCommandHandler _handler;

    public AddTransactionItemCommandHandlerTests()
    {
        _handler = new AddTransactionItemCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_AddsItemAndSaves()
    {
        var transaction = new Transaction("REF", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        await _handler.Handle(new AddTransactionItemCommand(transaction.Id, "PROD-1", 2, 10m), default);

        transaction.Items.Should().HaveCount(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_ReturnsUpdatedDto()
    {
        var transaction = new Transaction("REF", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var result = await _handler.Handle(new AddTransactionItemCommand(transaction.Id, "PROD-1", 3, 20m), default);

        result.TotalAmount.Should().Be(60m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenTransactionNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = async () => await _handler.Handle(new AddTransactionItemCommand(id, "PROD-1", 1, 10m), default);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_PropagatesException()
    {
        var transaction = new Transaction("REF", "USD", "key");
        transaction.AddItem("P1", 1, 10m);
        transaction.Submit("corr");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var act = async () => await _handler.Handle(
            new AddTransactionItemCommand(transaction.Id, "P2", 1, 5m), default);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Transaction.CannotModifyAfterSubmission");
    }
}
