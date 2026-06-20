using System.Text.Json;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payments.Domain.Events;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.BackgroundServices;

public class OutboxProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorService> logger)
    : BackgroundService
{
    private static readonly Dictionary<string, Func<string, IntegrationEvent?>> _typeMap = new()
    {
        [typeof(PaymentConfirmedEvent).FullName!] = payload =>
        {
            var evt = JsonSerializer.Deserialize<PaymentConfirmedEvent>(payload);
            if (evt is null) return null;
            return new PaymentConfirmedIntegrationEvent
            {
                PaymentId = evt.PaymentId,
                TransactionId = evt.TransactionId,
            };
        },
        [typeof(PaymentFailedEvent).FullName!] = payload =>
        {
            var evt = JsonSerializer.Deserialize<PaymentFailedEvent>(payload);
            if (evt is null) return null;
            return new PaymentFailedIntegrationEvent
            {
                PaymentId = evt.PaymentId,
                TransactionId = evt.TransactionId,
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
                logger.LogError(ex, "Unhandled error in payments outbox processor");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
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
                    if (integrationEvent is not null)
                    {
                        integrationEvent.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
                        await publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), ct);
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
