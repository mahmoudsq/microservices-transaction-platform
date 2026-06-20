using FluentAssertions;
using Transactions.Application.Commands.CreateTransaction;

namespace Transactions.Tests.Application.Validators;

public class CreateTransactionCommandValidatorTests
{
    private readonly CreateTransactionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var result = _validator.Validate(new CreateTransactionCommand("REF-001", "USD", "key-1"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyReference_FailsWithCode_Transaction_Reference_Required()
    {
        var result = _validator.Validate(new CreateTransactionCommand("", "USD", "key"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.Reference.Required");
    }

    [Fact]
    public void Validate_WithReferenceTooLong_FailsWithCode_Transaction_Reference_TooLong()
    {
        var longRef = new string('A', 101);
        var result = _validator.Validate(new CreateTransactionCommand(longRef, "USD", "key"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.Reference.TooLong");
    }

    [Fact]
    public void Validate_WithEmptyCurrency_FailsWithCode_Transaction_Currency_Required()
    {
        var result = _validator.Validate(new CreateTransactionCommand("REF", "", "key"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.Currency.Required");
    }

    [Fact]
    public void Validate_WithCurrencyNot3Chars_FailsWithCode_Transaction_Currency_MustBeThreeCharacters()
    {
        var result = _validator.Validate(new CreateTransactionCommand("REF", "US", "key"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.Currency.MustBeThreeCharacters");
    }

    [Fact]
    public void Validate_WithEmptyIdempotencyKey_FailsWithCode_Transaction_IdempotencyKey_Required()
    {
        var result = _validator.Validate(new CreateTransactionCommand("REF", "USD", ""));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.IdempotencyKey.Required");
    }

    [Fact]
    public void Validate_WithIdempotencyKeyTooLong_FailsWithCode_Transaction_IdempotencyKey_TooLong()
    {
        var longKey = new string('K', 101);
        var result = _validator.Validate(new CreateTransactionCommand("REF", "USD", longKey));
        result.Errors.Should().Contain(e => e.ErrorCode == "Transaction.IdempotencyKey.TooLong");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        var result = _validator.Validate(new CreateTransactionCommand("", "", ""));
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
