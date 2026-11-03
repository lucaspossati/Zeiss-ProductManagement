using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zeiss.ProductManagement.Infrastructure.Persistence;

namespace Zeiss.ProductManagement.Worker.BackgroundServices;

public sealed class DatabaseInitializationWorker : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<DatabaseInitializationWorker> _logger;

    public DatabaseInitializationWorker(
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime applicationLifetime,
        ILogger<DatabaseInitializationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!dbContext.Database.IsSqlServer())
        {
            throw new InvalidOperationException(
                "The database initialization worker requires a SQL Server connection string.");
        }

        _logger.LogInformation("Applying database migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);
        await SeedData.InitializeAsync(dbContext, cancellationToken);
        _logger.LogInformation("Database migrations and initialization completed.");

        _applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
