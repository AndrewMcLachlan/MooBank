using Asm.Domain;

namespace Asm.MooBank.Domain;

public static class SpecificationExtensions
{
    public static IQueryable<T> Apply<T>(this IQueryable<T> query, ISpecification<T> specification) where T : Entity =>
        specification.Apply(query);
}
