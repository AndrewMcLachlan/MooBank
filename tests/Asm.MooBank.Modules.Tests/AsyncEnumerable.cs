using System.Linq.Expressions;

namespace Asm.MooBank.Modules.Tests;
public class AsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)=>
        new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    public IAsyncEnumerator<T> GetEnumerator() =>
        new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
}

public class AsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> enumerator = enumerator ?? throw new ArgumentNullException();

    public T Current => enumerator.Current;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask<bool> MoveNextAsync()
    {
        var test = 1;
        bool result = enumerator.MoveNext();
        return ValueTask.FromResult(result);
    }
}
