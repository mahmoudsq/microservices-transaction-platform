using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Services;

public sealed class UnitOfWork(IApplicationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
