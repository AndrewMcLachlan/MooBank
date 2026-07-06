using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.StockHolding;

public class IncludeSpecification : ISpecification<StockHolding>
{
    public IQueryable<StockHolding> Apply(IQueryable<StockHolding> query) =>
        query.Include(s => s.Owners).ThenInclude(o => o.User)
             .Include(s => s.Owners).ThenInclude(o => o.Group)
             .Include(s => s.Viewers).ThenInclude(v => v.User)
             .Include(s => s.Viewers).ThenInclude(v => v.Group);
}
