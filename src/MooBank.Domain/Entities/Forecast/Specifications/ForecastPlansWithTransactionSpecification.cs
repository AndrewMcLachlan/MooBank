using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast.Specifications;

/// <summary>
/// Plans on which some planned item has claimed a given payment.
/// </summary>
public class ForecastPlansWithTransactionSpecification(Guid transactionId) : ISpecification<ForecastPlan>
{
    public IQueryable<ForecastPlan> Apply(IQueryable<ForecastPlan> query) =>
        query.Include(p => p.PlannedItems).ThenInclude(i => i.Transactions)
             .Where(p => p.PlannedItems.Any(i => i.Transactions.Any(t => t.TransactionId == transactionId)));
}
