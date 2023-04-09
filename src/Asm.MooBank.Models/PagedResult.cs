namespace Asm.MooBank.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Results { get; set; } = Enumerable.Empty<T>();

    public int Total { get; set; }
}
