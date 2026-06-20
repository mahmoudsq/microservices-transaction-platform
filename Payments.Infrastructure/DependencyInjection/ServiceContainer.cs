using BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Abstractions;
using Payments.Infrastructure.BackgroundServices;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Interceptors;
using Payments.Infrastructure.Persistence.Repositories;

namespace Payments.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<OutboxSaveChangesInterceptor>();

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure());
            options.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<PaymentsDbContext>());

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddHostedService<OutboxProcessorService>();
        services.AddMassTransitWithRabbitMq(config);

        return services;
    }
}
