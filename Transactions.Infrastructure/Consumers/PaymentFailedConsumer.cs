using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Transactions.Application.Commands.FailTransaction;

namespace Transactions.Infrastructure.Consumers;

public class PaymentFailedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentFailedConsumer> logger)
    : IConsumer<PaymentFailedIntegrationEvent>
{
    private const string ConsumerName = nameof(PaymentFailedConsumer);

    public async Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
    {
        var messageId = context.Message.Id;

        using var scope = scopeFactory.CreateScope();
        var idempotency = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (await idempotency.ExistsAsync(messageId, ConsumerName, context.CancellationToken))
        {
            logger.LogInformation("Duplicate PaymentFailed {MessageId} skipped", messageId);
            return;
        }

        await mediator.Send(
            new FailTransactionCommand(context.Message.TransactionId),
            context.CancellationToken);

        await idempotency.MarkProcessedAsync(messageId, ConsumerName, context.CancellationToken);
    }
}
