using System.Collections.Concurrent;
using Asm.MooBank.Models;

namespace Asm.MooBank.Queues;

public interface IImportTransactionsQueue
{
    void QueueImport(Guid instrumentId, Guid accountId, User user, byte[] fileData);

    Task<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

internal class ImportTransactionsQueue : IImportTransactionsQueue, IDisposable
{
    private readonly ConcurrentQueue<ImportWorkItem> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueImport(Guid instrumentId, Guid accountId, User user, byte[] fileData)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue(new ImportWorkItem(instrumentId, accountId, user, fileData));
        _signal.Release();
    }

    public async Task<ImportWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);

        return workItem!;
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
