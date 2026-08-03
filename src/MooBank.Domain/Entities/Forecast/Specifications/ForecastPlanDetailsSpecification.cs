using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast.Specifications;

public class ForecastPlanDetailsSpecification : ISpecification<ForecastPlan>
{
    public IQueryable<ForecastPlan> Apply(IQueryable<ForecastPlan> query) =>
        query
            .Include(p => p.Accounts).ThenInclude(a => a.Instrument)
            .Include(p => p.PlannedItems).ThenInclude(i => i.Tag)
            // Realisation reads these to decide an item's actuals, so a plan loaded without them
            // would silently fall back to tag matching.
            .Include(p => p.PlannedItems).ThenInclude(i => i.Transactions);
}
