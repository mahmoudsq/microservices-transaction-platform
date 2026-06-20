using System.Text.Json;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Transactions.Domain.Events;
using Transactions.Infrastructure.Persistence;

namespace Transactions.Infrastructure.BackgroundServices;

public class OutboxProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorService> logger)
    : BackgroundService
{
    private static readonly Dictionary<string, Func<string, IIntegrationEvent?>> _typeMap = new()
    {
        [typeof(TransactionSubmittedEvent).FullName!] = payload =>
        {
            var evt = JsonSerializer.Deserialize<TransactionSubmittedEvent>(payload);
            if (evt is null) return null;
            return new TransactionSubmittedIntegrationEvent
            {
                TransactionId = evt.TransactionId,
                TotalAmount = evt.TotalAmount,
                Currency = evt.Currency,
            };
        }
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error in outbox processor");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null
                && (m.LockedUntil == null || m.LockedUntil < DateTime.UtcNow))
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                message.LockedUntil = DateTime.UtcNow.AddSeconds(60);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                continue;
            }

            try
            {
                if (_typeMap.TryGetValue(message.Type, out var deserialize))
                {
                    var integrationEvent = deserialize(message.Payload);
                    if (integrationEvent is IntegrationEvent evt)
                    {
                        evt.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
                        await publishEndpoint.Publish(evt, evt.GetType(), ct);
                    }
                }

                message.ProcessedAt = DateTime.UtcNow;
                message.LockedUntil = null;
                logger.LogInformation(
                    "Published integration event {MessageType} for outbox message {MessageId}",
                    message.Type, message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process outbox message {MessageId} of type {MessageType}. RetryCount: {RetryCount}",
                    message.Id, message.Type, message.RetryCount);
                message.RetryCount++;
                message.LockedUntil = null;
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
