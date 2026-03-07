using System.Collections.Concurrent;
using Asm.MooBank.Models;

namespace Asm.MooBank.Queues;

public interface IReprocessTransactionsQueue
{
    void QueueReprocessTransactions(Guid instrumentId, Guid accountId);

    Task<ReprocessWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

internal class ReprocessTransactionsQueue : IReprocessTransactionsQueue, IDisposable
{
    private readonly ConcurrentQueue<ReprocessWorkItem> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueReprocessTransactions(Guid instrumentId, Guid accountId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue(new ReprocessWorkItem(instrumentId, accountId));
        _signal.Release();
    }

    public async Task<ReprocessWorkItem> DequeueAsync(CancellationToken cancellationToken)
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
