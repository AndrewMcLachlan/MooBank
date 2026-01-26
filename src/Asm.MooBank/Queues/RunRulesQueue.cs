using System.Collections.Concurrent;

namespace Asm.MooBank.Queues;

public interface IRunRulesQueue
{
    void QueueRunRules(Guid accountId);

    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}

internal class RunRulesQueue : IRunRulesQueue, IDisposable
{
    private readonly ConcurrentQueue<Guid> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;

    public void QueueRunRules(Guid accountId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _workItems.Enqueue(accountId);
        _signal.Release();
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
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
