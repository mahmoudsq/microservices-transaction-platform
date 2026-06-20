using BuildingBlocks.Abstractions;
using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Payments.Infrastructure.Persistence.Interceptors;

public class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var correlationId = dbContext is PaymentsDbContext px ? px.CorrelationId : null;

        var domainEvents = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            Type = domainEvent.GetType().FullName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            OccurredAt = domainEvent.OccurredOn,
            ServiceName = "payments-service",
            CorrelationId = correlationId
        });

        await dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);

        foreach (var entry in dbContext.ChangeTracker.Entries<IHasDomainEvents>())
            entry.Entity.ClearDomainEvents();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
