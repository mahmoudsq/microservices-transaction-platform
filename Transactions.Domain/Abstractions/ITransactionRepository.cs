using Transactions.Domain.Entities;

namespace Transactions.Domain.Abstractions;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
}
