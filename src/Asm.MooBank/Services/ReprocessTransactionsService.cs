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
                var (instrumentId, accountId) = await _taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var importerFactory = scope.ServiceProvider.GetRequiredService<IImporterFactory>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var importer = await importerFactory.Create(instrumentId, accountId, cancellationToken) ?? throw new InvalidOperationException($"Import is not supported for account with ID: {accountId}");

                    await importer.Reprocess(instrumentId, accountId, cancellationToken);

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred reprocessing transactions for account {AccountId}.", accountId);
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
    void QueueReprocessTransactions(Guid instrumentId, Guid accountId);

    Task<(Guid InstrumentId, Guid AccountId)> DequeueAsync(CancellationToken cancellationToken);
}

public class ReprocessTransactionsQueue : IReprocessTransactionsQueue, IDisposable
{
    private readonly ConcurrentQueue<(Guid, Guid)> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueReprocessTransactions(Guid instrumentId, Guid accountId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue((instrumentId, accountId));
        _signal.Release();
    }

    public async Task<(Guid, Guid)> DequeueAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _signal.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
