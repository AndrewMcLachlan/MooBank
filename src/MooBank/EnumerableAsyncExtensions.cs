namespace Asm.MooBank;

public static class EnumerableAsyncExtensions
{
    /// <summary>
    /// Projects each element of a sequence using an asynchronous selector, awaiting each element in turn.
    /// </summary>
    /// <remarks>
    /// Elements are awaited sequentially, making this safe for selectors that share a DbContext.
    /// </remarks>
    public static async Task<List<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
    {
        List<TResult> results = source.TryGetNonEnumeratedCount(out var count) ? new(count) : [];

        foreach (var item in source)
        {
            results.Add(await selector(item));
        }

        return results;
    }
}
