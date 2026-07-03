using Payments.Domain.Entities;

namespace Payments.Domain.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
}
