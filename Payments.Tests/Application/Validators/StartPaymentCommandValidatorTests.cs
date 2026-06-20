using FluentAssertions;
using Payments.Application.Commands.StartPayment;

namespace Payments.Tests.Application.Validators;

public class StartPaymentCommandValidatorTests
{
    private readonly StartPaymentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), 100m, "USD", "corr-1"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTransactionId_FailsWithCode_Payment_TransactionId_Required()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.Empty, 100m, "USD", "corr"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.TransactionId.Required");
    }

    [Fact]
    public void Validate_WithZeroAmount_FailsWithCode_Payment_Amount_MustBePositive()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), 0m, "USD", "corr"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.Amount.MustBePositive");
    }

    [Fact]
    public void Validate_WithNegativeAmount_FailsWithCode_Payment_Amount_MustBePositive()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), -10m, "USD", "corr"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.Amount.MustBePositive");
    }

    [Fact]
    public void Validate_WithEmptyCurrency_FailsWithCode_Payment_Currency_Required()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), 100m, "", "corr"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.Currency.Required");
    }

    [Fact]
    public void Validate_WithCurrencyNot3Chars_FailsWithCode_Payment_Currency_MustBeThreeCharacters()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), 100m, "US", "corr"));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.Currency.MustBeThreeCharacters");
    }

    [Fact]
    public void Validate_WithEmptyCorrelationId_FailsWithCode_Payment_CorrelationId_Required()
    {
        var result = _validator.Validate(new StartPaymentCommand(Guid.NewGuid(), 100m, "USD", ""));
        result.Errors.Should().Contain(e => e.ErrorCode == "Payment.CorrelationId.Required");
    }
}
