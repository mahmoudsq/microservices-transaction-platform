using FluentAssertions;
using NSubstitute;
using Transactions.Domain.Abstractions;
using Transactions.Application.Queries.GetAllTransactions;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Application.Queries;

public class GetAllTransactionsQueryHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly GetAllTransactionsQueryHandler _handler;

    public GetAllTransactionsQueryHandlerTests()
    {
        _handler = new GetAllTransactionsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenTransactionsExist_ReturnsMappedDtos()
    {
        var transactions = new List<Transaction>
        {
            new("REF-1", "USD", "key-1"),
            new("REF-2", "EUR", "key-2")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(transactions);

        var result = await _handler.Handle(new GetAllTransactionsQuery(), default);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoTransactions_ReturnsEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Transaction>());

        var result = await _handler.Handle(new GetAllTransactionsQuery(), default);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsDtoCountMatchingRepository()
    {
        var transactions = Enumerable.Range(1, 5)
            .Select(i => new Transaction($"REF-{i}", "USD", $"key-{i}"))
            .ToList();
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(transactions);

        var result = await _handler.Handle(new GetAllTransactionsQuery(), default);

        result.Should().HaveCount(5);
    }
}
