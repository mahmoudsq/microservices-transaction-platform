using FluentValidation;

namespace Transactions.Application.Commands.AddTransactionItem;

public class AddTransactionItemCommandValidator : AbstractValidator<AddTransactionItemCommand>
{
    public AddTransactionItemCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithErrorCode("Transaction.Id.Required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithErrorCode("TransactionItem.ProductId.Required")
            .MaximumLength(100).WithErrorCode("TransactionItem.ProductId.TooLong");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithErrorCode("TransactionItem.Quantity.MustBePositive");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithErrorCode("TransactionItem.Price.MustBePositive");
    }
}
