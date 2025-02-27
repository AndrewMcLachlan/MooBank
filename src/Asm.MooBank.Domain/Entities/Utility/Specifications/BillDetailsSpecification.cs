using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Utility.Specifications;

public class BillDetailsSpecification : ISpecification<Account>
{
    public IQueryable<Account> Apply(IQueryable<Account> query) =>
        query.Include(a => a.Bills).ThenInclude(b => b.Periods).ThenInclude(p => p.Usage)
             .Include(a => a.Bills).ThenInclude(b => b.Periods).ThenInclude(p => p.ServiceCharge)
             .Include(a => a.Bills).ThenInclude(b => b.Discounts);
}
