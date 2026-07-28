using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Retirement.Specifications;

/// <summary>
/// Loads a plan with its members and their instrument links, for editing.
/// </summary>
/// <remarks>
/// Deliberately stops at the link rows: editing a plan never reads an instrument, and joining to
/// the required <c>Instrument</c> navigation costs every caller a join it does not need. Use
/// <see cref="RetirementPlanProjectionSpecification"/> where balances are actually required.
/// </remarks>
public class RetirementPlanDetailsSpecification : ISpecification<RetirementPlan>
{
    public IQueryable<RetirementPlan> Apply(IQueryable<RetirementPlan> query) =>
        query
            .Include(p => p.Members).ThenInclude(m => m.Accounts);
}

/// <summary>
/// Loads a plan with everything a projection needs, including each linked instrument so its
/// balance can be read.
/// </summary>
public class RetirementPlanProjectionSpecification : ISpecification<RetirementPlan>
{
    public IQueryable<RetirementPlan> Apply(IQueryable<RetirementPlan> query) =>
        query
            .Include(p => p.Members).ThenInclude(m => m.Accounts).ThenInclude(a => a.Instrument);
}
