using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Utility.Specifications;

public class BillDetailsSpecification : ISpecification<Bill>
{
    public IQueryable<Bill> Apply(IQueryable<Bill> query) =>
        query.Include(b => b.Periods).ThenInclude(p => p.Usage)
             .Include(b => b.Periods).ThenInclude(p => p.ServiceCharges).ThenInclude(sc => sc.ChargeType)
             .Include(b => b.Discounts);
}
