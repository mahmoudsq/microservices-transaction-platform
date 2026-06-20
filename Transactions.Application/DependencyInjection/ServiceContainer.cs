using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Transactions.Application.Behaviors;

namespace Transactions.Application.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceContainer).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceContainer).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
