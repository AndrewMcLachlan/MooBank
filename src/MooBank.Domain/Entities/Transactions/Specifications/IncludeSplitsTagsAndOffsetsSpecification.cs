using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

/// <summary>
/// Splits with their tags and settings, plus both sides of the offsets, for a report that nets
/// amounts and has to know which splits are excluded from reporting.
/// </summary>
public class IncludeSplitsTagsAndOffsetsSpecification : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(s => s.Tags).ThenInclude(tag => tag.Settings)
             .Include(t => t.Splits).ThenInclude(s => s.OffsetBy)
             .Include(t => t.OffsetFor);
}
