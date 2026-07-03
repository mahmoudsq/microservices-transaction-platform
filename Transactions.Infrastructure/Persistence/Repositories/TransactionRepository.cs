using Microsoft.EntityFrameworkCore;
using Transactions.Domain.Abstractions;
using Transactions.Domain.Entities;

namespace Transactions.Infrastructure.Persistence.Repositories;

public class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => context.Transactions.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
        => context.Transactions.Include(x => x.Items).FirstOrDefaultAsync(x => x.IdempotencyKey == key, ct);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default)
        => await context.Transactions.Include(x => x.Items).AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await context.Transactions.AddAsync(transaction, ct);
}
