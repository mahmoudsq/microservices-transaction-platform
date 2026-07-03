using Microsoft.EntityFrameworkCore;
using Payments.Domain.Abstractions;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence.Repositories;

public class PaymentRepository(PaymentsDbContext context) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => context.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Payment?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default)
        => context.Payments.FirstOrDefaultAsync(x => x.TransactionId == transactionId, ct);

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
        => await context.Payments.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await context.Payments.AddAsync(payment, ct);
}
