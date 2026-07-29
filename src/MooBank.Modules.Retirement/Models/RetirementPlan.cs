using System.ComponentModel;

namespace Asm.MooBank.Modules.Retirement.Models;

[DisplayName("SimpleRetirementPlan")]
public record RetirementPlanBase
{
    public required string Name { get; init; }

    /// <summary>
    /// Assumed nominal return on the balance, as a rate (0.065 is 6.5% a year). Applies to members
    /// whose growth strategy is <see cref="GrowthStrategy.Custom"/>; the named strategies carry
    /// their own assumed return.
    /// </summary>
    public decimal ExpectedReturnRate { get; init; }

    /// <summary>
    /// Assumed inflation, as a rate. Also stands in for wage growth.
    /// </summary>
    public decimal InflationRate { get; init; }

    /// <summary>
    /// Employer superannuation guarantee rate applied to each member's income.
    /// </summary>
    public decimal SuperGuaranteeRate { get; init; }

    /// <summary>
    /// Tax withheld on concessional contributions entering the fund.
    /// </summary>
    public decimal ContributionsTaxRate { get; init; }

    /// <summary>
    /// The age savings are assumed to need to last until.
    /// </summary>
    public int LifeExpectancy { get; init; }

    /// <summary>
    /// What the household intends to live on each year in retirement, in today's dollars. Drawn from
    /// the members' balances once they have all retired.
    /// </summary>
    public decimal TargetRetirementIncome { get; init; }

    /// <summary>
    /// How many years before retiring a member's balance moves to cash. Nought turns the glide off.
    /// </summary>
    public int PreRetirementSwitchYears { get; init; }

    /// <summary>
    /// The nominal return a balance earns once it has moved to cash.
    /// </summary>
    public decimal CashReturnRate { get; init; }

    public IEnumerable<RetirementPlanMember> Members { get; init; } = [];
}

[DisplayName("RetirementPlan")]
public sealed record RetirementPlan : RetirementPlanBase
{
    public Guid Id { get; init; }

    public DateTime CreatedUtc { get; init; }

    public DateTime UpdatedUtc { get; init; }
}

[DisplayName("RetirementPlanMember")]
public sealed record RetirementPlanMember
{
    /// <summary>
    /// Empty when creating a member; carries the existing id when updating one.
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// The person this member is, which must be someone in the plan's family.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// The person's name, read from their user record. Ignored on the way in.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The member's age now. Held rather than a date of birth so the application does not store
    /// personal information it has no use for.
    /// </summary>
    public int CurrentAge { get; init; }

    /// <summary>
    /// Current gross annual income, which drives employer contributions.
    /// </summary>
    public decimal CurrentIncome { get; init; }

    /// <summary>
    /// Additional concessional contributions made from pre-tax income each year.
    /// </summary>
    public decimal SalarySacrifice { get; init; }

    public int RetirementAge { get; init; }

    /// <summary>
    /// Administration fees charged by the fund each year.
    /// </summary>
    public decimal AnnualFees { get; init; }

    /// <summary>
    /// Insurance premiums deducted from the balance each year.
    /// </summary>
    public decimal InsurancePremium { get; init; }

    public GrowthStrategy GrowthStrategy { get; init; }

    /// <summary>
    /// The superannuation instruments belonging to this member.
    /// </summary>
    public IEnumerable<Guid> InstrumentIds { get; init; } = [];
}
