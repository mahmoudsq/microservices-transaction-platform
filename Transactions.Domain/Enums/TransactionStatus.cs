namespace Transactions.Domain.Enums;

public enum TransactionStatus
{
    Draft     = 1,
    Submitted = 2,
    Completed = 3,
    Failed    = 4,
    Cancelled = 5
}
