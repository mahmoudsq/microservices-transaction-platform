using BuildingBlocks.Correlation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Transactions.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TransactionsDbContext>
{
    public TransactionsDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbContextOptionsBuilder<TransactionsDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConnection"))
            .Options;

        return new TransactionsDbContext(options, new CorrelationContext("migrations"));
    }
}
