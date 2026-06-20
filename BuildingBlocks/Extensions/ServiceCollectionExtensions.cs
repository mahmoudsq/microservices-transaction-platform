using BuildingBlocks.Abstractions;
using BuildingBlocks.Correlation;
using BuildingBlocks.Idempotency;
using BuildingBlocks.Localization;
using BuildingBlocks.Outbox;
using BuildingBlocks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BuildingBlocks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonLocalization(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContext>(sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var correlationId = httpContext?.Items["X-Correlation-ID"]?.ToString()
                                ?? Guid.NewGuid().ToString();

            return new CorrelationContext(correlationId);
        });

        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
