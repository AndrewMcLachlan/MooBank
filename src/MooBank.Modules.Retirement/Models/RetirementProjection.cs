using System.ComponentModel;

namespace Asm.MooBank.Modules.Retirement.Models;

[DisplayName("RetirementProjection")]
public sealed record RetirementProjection
{
    public Guid PlanId { get; init; }

    /// <summary>
    /// Combined household position for each year of the projection.
    /// </summary>
    public required IEnumerable<RetirementProjectionYear> Years { get; init; }

    /// <summary>
    /// One outcome per member, in the order the members are held on the plan.
    /// </summary>
    public required IEnumerable<RetirementMemberOutcome> Members { get; init; }

    public required RetirementProjectionSummary Summary { get; init; }
}

[DisplayName("RetirementProjectionYear")]
public sealed record RetirementProjectionYear
{
    public int Year { get; init; }

    public decimal OpeningBalance { get; init; }

    /// <summary>
    /// Employer contributions for the year, after contributions tax.
    /// </summary>
    public decimal Contributions { get; init; }

    public decimal InvestmentReturn { get; init; }

    public decimal ClosingBalance { get; init; }

    /// <summary>
    /// <see cref="ClosingBalance"/> discounted back to the purchasing power of the projection's
    /// first year, so the curve can be read without mentally removing inflation.
    /// </summary>
    public decimal ClosingBalanceInTodaysDollars { get; init; }

    /// <summary>
    /// True once every member has reached their retirement age, so contributions have stopped.
    /// </summary>
    public bool AllRetired { get; init; }
}

[DisplayName("RetirementMemberOutcome")]
public sealed record RetirementMemberOutcome
{
    public Guid MemberId { get; init; }

    public required string Name { get; init; }

    public int CurrentAge { get; init; }

    public int RetirementAge { get; init; }

    public int YearsToRetirement { get; init; }

    public int RetirementYear { get; init; }

    /// <summary>
    /// The member's combined superannuation balance across their selected accounts today.
    /// </summary>
    public decimal CurrentBalance { get; init; }

    public decimal BalanceAtRetirement { get; init; }

    public decimal BalanceAtRetirementInTodaysDollars { get; init; }

    /// <summary>
    /// The level annual income, in today's dollars, that the balance at retirement supports from
    /// retirement until the plan's life expectancy.
    /// </summary>
    public decimal AnnualRetirementIncomeInTodaysDollars { get; init; }

    /// <summary>
    /// True when the member has already reached their retirement age, so no accumulation is
    /// projected for them.
    /// </summary>
    public bool AlreadyRetired { get; init; }

    /// <summary>
    /// The investment option this member's balance was projected under.
    /// </summary>
    public GrowthStrategy GrowthStrategy { get; init; }

    /// <summary>
    /// The nominal return the strategy implies. Surfaced so the page can show what a strategy
    /// actually means without the caller having to know the table.
    /// </summary>
    public decimal ReturnRate { get; init; }
}

[DisplayName("RetirementProjectionSummary")]
public sealed record RetirementProjectionSummary
{
    public decimal CurrentBalance { get; init; }

    /// <summary>
    /// Household balance in the year the last member retires.
    /// </summary>
    public decimal BalanceAtRetirement { get; init; }

    public decimal BalanceAtRetirementInTodaysDollars { get; init; }

    /// <summary>
    /// Combined level annual household income in today's dollars.
    /// </summary>
    public decimal AnnualRetirementIncomeInTodaysDollars { get; init; }

    /// <summary>
    /// The year the last member reaches their retirement age.
    /// </summary>
    public int RetirementYear { get; init; }

    /// <summary>
    /// The real (above-inflation) return implied by the plan's return and inflation assumptions.
    /// </summary>
    public decimal RealReturnRate { get; init; }
}
