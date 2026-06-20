using FluentAssertions;
using Transactions.Application.Commands.AddTransactionItem;

namespace Transactions.Tests.Application.Validators;

public class AddTransactionItemCommandValidatorTests
{
    private readonly AddTransactionItemCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), "PROD-1", 2, 10m));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTransactionId_FailsWithCode_Transaction_Id_Required()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.Empty, "PROD-1", 1, 10m));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.Id.Required");
    }

    [Fact]
    public void Validate_WithEmptyProductId_FailsWithCode_TransactionItem_ProductId_Required()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), "", 1, 10m));
        result.Errors.Should().Contain(e => e.ErrorCode == "TransactionItem.ProductId.Required");
    }

    [Fact]
    public void Validate_WithProductIdTooLong_FailsWithCode_TransactionItem_ProductId_TooLong()
    {
        var longId = new string('P', 101);
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), longId, 1, 10m));
        result.Errors.Should().Contain(e => e.ErrorCode == "TransactionItem.ProductId.TooLong");
    }

    [Fact]
    public void Validate_WithZeroQuantity_FailsWithCode_TransactionItem_Quantity_MustBePositive()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), "PROD-1", 0, 10m));
        result.Errors.Should().Contain(e => e.ErrorCode == "TransactionItem.Quantity.MustBePositive");
    }

    [Fact]
    public void Validate_WithZeroPrice_FailsWithCode_TransactionItem_Price_MustBePositive()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), "PROD-1", 1, 0m));
        result.Errors.Should().Contain(e => e.ErrorCode == "TransactionItem.Price.MustBePositive");
    }

    [Fact]
    public void Validate_WithNegativeQuantityAndPrice_ReturnsMultipleErrors()
    {
        var result = _validator.Validate(new AddTransactionItemCommand(Guid.NewGuid(), "PROD-1", -1, -5m));
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
