using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

/// <remarks>
/// OffsetFor must stay included for a delete: the database has no cascade from Transaction to
/// TransactionSplitOffset — a second cascade path, which SQL Server rejects — so EF can only remove
/// those rows if it is tracking them. Dropped from here, a delete fails on the foreign key.
/// </remarks>
public class IncludeSplitsAndOffsetsSpecification : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(t => t.OffsetBy).Include(t => t.OffsetFor);
}
