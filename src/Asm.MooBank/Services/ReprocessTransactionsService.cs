using System.Collections.Concurrent;
using Asm.Domain;
using Asm.MooBank.Importers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Services;

public class ReprocessTransactionsService(IReprocessTransactionsQueue taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ReprocessTransactionsService>();
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IReprocessTransactionsQueue _taskQueue = taskQueue;

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reprocess Service is starting.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var accountId = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var importerFactory = scope.ServiceProvider.GetRequiredService<IImporterFactory>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var importer = await importerFactory.Create(accountId, cancellationToken) ?? throw new InvalidOperationException($"Import is not supported for account with ID: {accountId}");

                    await importer.Reprocess(accountId, cancellationToken);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred reprocessing transactions for account {AccountId}.", nameof(accountId));
                }
            }

            _logger.LogInformation("Reprocess Service is stopping.");
        }
        catch (TaskCanceledException tcex)
        {
            _logger.LogWarning(tcex, "Reprocess Service is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reprocess Service encountered an error: {Message}", ex.Message);
        }
    }
}

public interface IReprocessTransactionsQueue
{
    void QueueReprocessTransactions(Guid accountId);

    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}

public class ReprocessTransactionsQueue : IReprocessTransactionsQueue
{
    private readonly ConcurrentQueue<Guid> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void QueueReprocessTransactions(Guid accountId)
    {
        _workItems.Enqueue(accountId);
        _signal.Release();
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }
}
