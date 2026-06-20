using BuildingBlocks.Exceptions;
using FluentAssertions;
using Transactions.Domain.Entities;

namespace Transactions.Tests.Domain;

public class TransactionItemTests
{
    [Fact]
    public void Constructor_WithValidData_SetsAllProperties()
    {
        var transactionId = Guid.NewGuid();
        var item = new TransactionItem(transactionId, "PROD-1", 3, 15m);

        item.TransactionId.Should().Be(transactionId);
        item.ProductId.Should().Be("PROD-1");
        item.Quantity.Should().Be(3);
        item.Price.Should().Be(15m);
    }

    [Fact]
    public void Constructor_WithValidData_AssignsNonEmptyId()
    {
        var item = new TransactionItem(Guid.NewGuid(), "PROD-1", 1, 10m);
        item.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Subtotal_IsPriceMultipliedByQuantity()
    {
        var item = new TransactionItem(Guid.NewGuid(), "PROD-1", 4, 12.50m);
        item.Subtotal.Should().Be(50m);
    }

    [Fact]
    public void Constructor_WithEmptyProductId_ThrowsDomainException_ProductIdRequired()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "", 1, 10m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.ProductId.Required");
    }

    [Fact]
    public void Constructor_WithWhitespaceProductId_ThrowsDomainException()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "   ", 1, 10m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.ProductId.Required");
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ThrowsDomainException_QuantityMustBePositive()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "PROD-1", 0, 10m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.Quantity.MustBePositive");
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ThrowsDomainException()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "PROD-1", -1, 10m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.Quantity.MustBePositive");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ThrowsDomainException_PriceMustBePositive()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "PROD-1", 1, 0m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.Price.MustBePositive");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsDomainException()
    {
        var act = () => new TransactionItem(Guid.NewGuid(), "PROD-1", 1, -5m);
        act.Should().Throw<DomainException>().WithMessage("TransactionItem.Price.MustBePositive");
    }
}
