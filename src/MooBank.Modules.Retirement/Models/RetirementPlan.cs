using System.ComponentModel;

namespace Asm.MooBank.Modules.Retirement.Models;

[DisplayName("SimpleRetirementPlan")]
public record RetirementPlanBase
{
    public required string Name { get; init; }

    /// <summary>
    /// Assumed nominal return on the balance, as a rate (0.065 is 6.5% a year).
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

    public required string Name { get; init; }

    public DateOnly DateOfBirth { get; init; }

    /// <summary>
    /// Current gross annual income, which drives employer contributions.
    /// </summary>
    public decimal CurrentIncome { get; init; }

    public int RetirementAge { get; init; }

    /// <summary>
    /// The superannuation instruments belonging to this member.
    /// </summary>
    public IEnumerable<Guid> InstrumentIds { get; init; } = [];
}
