using BuildingBlocks.Correlation;
using BuildingBlocks.Extensions;
using BuildingBlocks.Middleware;
using Scalar.AspNetCore;
using Serilog;
using Transactions.Application.DependencyInjection;
using Transactions.Infrastructure.DependencyInjection;
using Transactions.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("Service", "transactions-service")
              .WriteTo.Console(outputTemplate:
                  "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddJsonLocalization();

    builder.Services.AddInfrastructure(builder.Configuration)
        .AddApplication()
        .AddMessaging();

    builder.Services
        .AddHealthChecks()
        .AddDbContextCheck<TransactionsDbContext>("transactions-db")
        .AddCheck("rabbitmq", () =>
        {
            try
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
                    UserName = builder.Configuration["RabbitMQ:Username"] ?? "guest",
                    Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
                };
                using var conn = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
            }
            catch
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("RabbitMQ unreachable");
            }
        });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseRequestLocalization(options =>
        options.SetDefaultCulture("en")
               .AddSupportedCultures("en", "ar")
               .AddSupportedUICultures("en", "ar"));

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<DomainExceptionMiddleware>();
    app.UseMiddleware<CorrelationMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Transactions service failed to start");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
