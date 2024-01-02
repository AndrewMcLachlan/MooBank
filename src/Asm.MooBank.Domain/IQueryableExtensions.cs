namespace Asm.MooBank.Domain;
public static class IQueryableExtensions
{
    public static IQueryable<T> Specify<T>(this IQueryable<T> query, ISpecification<T> specification) where T : Entity
    {
        return specification == null ? query : specification.Apply(query);
    }
}
