using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

public class IncludeTagsSpecification : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.Tags);
}
