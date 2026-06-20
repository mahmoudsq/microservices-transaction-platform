using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Payments.Application.Commands.StartPayment;

namespace Payments.Infrastructure.Consumers;

public class TransactionSubmittedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionSubmittedConsumer> logger)
    : IConsumer<TransactionSubmittedIntegrationEvent>
{
    private const string ConsumerName = nameof(TransactionSubmittedConsumer);

    public async Task Consume(ConsumeContext<TransactionSubmittedIntegrationEvent> context)
    {
        var messageId = context.Message.Id;

        using var scope = scopeFactory.CreateScope();
        var idempotency = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (await idempotency.ExistsAsync(messageId, ConsumerName, context.CancellationToken))
        {
            logger.LogInformation("Duplicate message {MessageId} skipped", messageId);
            return;
        }

        var correlationId = context.Message.CorrelationId
            ?? context.CorrelationId?.ToString()
            ?? Guid.NewGuid().ToString();

        await mediator.Send(new StartPaymentCommand(
            context.Message.TransactionId,
            context.Message.TotalAmount,
            context.Message.Currency,
            correlationId), context.CancellationToken);

        await idempotency.MarkProcessedAsync(messageId, ConsumerName, context.CancellationToken);
    }
}
