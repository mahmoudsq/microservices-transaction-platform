using FluentValidation;

namespace Payments.Application.Commands.StartPayment;

public class StartPaymentCommandValidator : AbstractValidator<StartPaymentCommand>
{
    public StartPaymentCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithErrorCode("Payment.TransactionId.Required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithErrorCode("Payment.Amount.MustBePositive");

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode("Payment.Currency.Required")
            .Length(3).WithErrorCode("Payment.Currency.MustBeThreeCharacters");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithErrorCode("Payment.CorrelationId.Required");
    }
}
