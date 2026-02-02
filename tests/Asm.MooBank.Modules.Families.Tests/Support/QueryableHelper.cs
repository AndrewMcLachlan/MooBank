using Asm.Testing.Domain;

namespace Asm.MooBank.Modules.Families.Tests.Support;

internal static class QueryableHelper
{
    public static IQueryable<T> CreateAsyncQueryable<T>(IEnumerable<T> data) where T : class
        => MockDbSetFactory.CreateQueryable(data);
}
