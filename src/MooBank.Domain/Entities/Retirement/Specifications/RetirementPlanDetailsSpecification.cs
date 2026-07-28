using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement.Specifications;

/// <summary>
/// Loads a plan with its members and each member's linked instruments.
/// </summary>
/// <remarks>
/// The instrument is included because a projection needs its balance; the balance itself comes
/// from the auto-included <c>BalanceInfo</c> navigation on <c>TransactionInstrument</c>.
/// </remarks>
public class RetirementPlanDetailsSpecification : ISpecification<RetirementPlan>
{
    public IQueryable<RetirementPlan> Apply(IQueryable<RetirementPlan> query) =>
        query
            .Include(p => p.Members).ThenInclude(m => m.Accounts).ThenInclude(a => a.Instrument);
}
