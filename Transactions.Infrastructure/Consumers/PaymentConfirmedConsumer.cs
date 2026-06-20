using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transactions.Application.Commands.CompleteTransaction;

namespace Transactions.Infrastructure.Consumers;

public class PaymentConfirmedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentConfirmedConsumer> logger)
    : IConsumer<PaymentConfirmedIntegrationEvent>
{
    private const string ConsumerName = nameof(PaymentConfirmedConsumer);

    public async Task Consume(ConsumeContext<PaymentConfirmedIntegrationEvent> context)
    {
        var messageId = context.Message.Id;

        using var scope = scopeFactory.CreateScope();
        var idempotency = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (await idempotency.ExistsAsync(messageId, ConsumerName, context.CancellationToken))
        {
            logger.LogInformation("Duplicate PaymentConfirmed {MessageId} skipped", messageId);
            return;
        }

        await mediator.Send(
            new CompleteTransactionCommand(context.Message.TransactionId),
            context.CancellationToken);

        await idempotency.MarkProcessedAsync(messageId, ConsumerName, context.CancellationToken);
    }
}
