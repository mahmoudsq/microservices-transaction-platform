using BuildingBlocks.Abstractions;
using FluentAssertions;
using NSubstitute;
using Transactions.Application.Abstractions;
using Transactions.Application.Commands.CreateTransaction;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Application.Commands;

public class CreateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenIdempotencyKeyExists_ReturnsExistingTransaction_WithoutSaving()
    {
        var existing = new Transaction("REF", "USD", "key-1");
        _repository.GetByIdempotencyKeyAsync("key-1", Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _handler.Handle(new CreateTransactionCommand("REF", "USD", "key-1"), default);

        result.IdempotencyKey.Should().Be("key-1");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewKey_CreatesTransaction_AndSaves()
    {
        _repository.GetByIdempotencyKeyAsync("new-key", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        await _handler.Handle(new CreateTransactionCommand("REF", "USD", "new-key"), default);

        await _repository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewKey_ReturnsTransactionDto()
    {
        _repository.GetByIdempotencyKeyAsync("new-key", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var result = await _handler.Handle(new CreateTransactionCommand("REF-001", "USD", "new-key"), default);

        result.Should().NotBeNull();
        result.Reference.Should().Be("REF-001");
        result.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Handle_WhenNewKey_CallsAddAsync()
    {
        _repository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        await _handler.Handle(new CreateTransactionCommand("REF", "EUR", "key-x"), default);

        await _repository.Received(1).AddAsync(
            Arg.Is<Transaction>(t => t.Reference == "REF" && t.Currency == "EUR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewKey_CallsSaveChangesAsync()
    {
        _repository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        await _handler.Handle(new CreateTransactionCommand("REF", "USD", "key"), default);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
