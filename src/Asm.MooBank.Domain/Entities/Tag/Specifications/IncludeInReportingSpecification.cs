namespace Asm.MooBank.Domain.Entities.Tag.Specifications;
public class IncludeInReportingSpecification : ISpecification<Tag>
{
    public IQueryable<Tag> Apply(IQueryable<Tag> query) =>
        query.Where(t => !t.Deleted && (t.Settings == null || !t.Settings.ExcludeFromReporting));
}
