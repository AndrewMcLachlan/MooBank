using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.StockHolding;

public class IncludeSpecification : ISpecification<StockHolding>
{
    public IQueryable<StockHolding> Apply(IQueryable<StockHolding> query) =>
        query.Include(s => s.Owners).Include(s => s.Viewers);
}
