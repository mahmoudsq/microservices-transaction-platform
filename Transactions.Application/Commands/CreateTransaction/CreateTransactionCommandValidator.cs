using FluentValidation;

namespace Transactions.Application.Commands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Reference)
            .NotEmpty().WithErrorCode("Transaction.Reference.Required")
            .MaximumLength(100).WithErrorCode("Transaction.Reference.TooLong");

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode("Transaction.Currency.Required")
            .Length(3).WithErrorCode("Transaction.Currency.MustBeThreeCharacters");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithErrorCode("Transaction.IdempotencyKey.Required")
            .MaximumLength(100).WithErrorCode("Transaction.IdempotencyKey.TooLong");
    }
}
