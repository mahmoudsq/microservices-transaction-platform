using FluentAssertions;
using Transactions.Application.DTOs;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Application.DTOs;

public class TransactionMappingExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllScalarProperties()
    {
        var transaction = new Transaction("REF-001", "EUR", "idem-key");

        var dto = transaction.ToDto();

        dto.Id.Should().Be(transaction.Id);
        dto.Reference.Should().Be("REF-001");
        dto.Currency.Should().Be("EUR");
        dto.IdempotencyKey.Should().Be("idem-key");
        dto.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public void ToDto_MapsItemsCollection()
    {
        var transaction = new Transaction("REF", "USD", "key");
        transaction.AddItem("PROD-A", 2, 10m);
        transaction.AddItem("PROD-B", 1, 5m);

        var dto = transaction.ToDto();

        dto.Items.Should().HaveCount(2);
        dto.TotalAmount.Should().Be(25m);
    }

    [Fact]
    public void ToDto_WithNoItems_ReturnsEmptyItemsCollection()
    {
        var transaction = new Transaction("REF", "USD", "key");

        var dto = transaction.ToDto();

        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_Status_IsStringRepresentation()
    {
        var transaction = new Transaction("REF", "USD", "key");

        var dto = transaction.ToDto();

        dto.Status.Should().Be("Draft");
    }
}
