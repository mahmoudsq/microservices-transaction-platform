using BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Application.Abstractions;
using Transactions.Infrastructure.BackgroundServices;
using Transactions.Infrastructure.Persistence;
using Transactions.Infrastructure.Persistence.Interceptors;
using Transactions.Infrastructure.Persistence.Repositories;

namespace Transactions.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddSingleton<OutboxSaveChangesInterceptor>();

        services.AddDbContext<TransactionsDbContext>((sp, options) =>
        {
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure());
            options.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<TransactionsDbContext>());

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddHostedService<OutboxProcessorService>();
        services.AddMassTransitWithRabbitMq(config);

        return services;
    }
}
