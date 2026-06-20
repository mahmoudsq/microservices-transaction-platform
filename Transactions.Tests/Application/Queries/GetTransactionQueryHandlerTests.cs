using BuildingBlocks.Exceptions;
using FluentAssertions;
using NSubstitute;
using Transactions.Application.Abstractions;
using Transactions.Application.Queries.GetTransaction;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Application.Queries;

public class GetTransactionQueryHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly GetTransactionQueryHandler _handler;

    public GetTransactionQueryHandlerTests()
    {
        _handler = new GetTransactionQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenTransactionFound_ReturnsDto()
    {
        var transaction = new Transaction("REF-001", "USD", "key");
        _repository.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);

        var result = await _handler.Handle(new GetTransactionQuery(transaction.Id), default);

        result.Should().NotBeNull();
        result!.Reference.Should().Be("REF-001");
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsDomainException_TransactionNotFound()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var act = async () => await _handler.Handle(new GetTransactionQuery(id), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Transaction.NotFound");
    }
}
