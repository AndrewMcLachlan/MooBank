using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Transactions.Specifications;

public class IncludeAllSpecification : ISpecification<Transaction>
{
    public IQueryable<Transaction> Apply(IQueryable<Transaction> query) =>
        query.Include(t => t.Splits).ThenInclude(ts => ts.Tags)
             .Include(t => t.Splits).ThenInclude(ts => ts.OffsetBy).ThenInclude(to => to.OffsetByTransaction)
             .Include(t => t.OffsetFor).ThenInclude(t => t.TransactionSplit).ThenInclude(t => t.Transaction)
             .Include(t => t.User);
}
