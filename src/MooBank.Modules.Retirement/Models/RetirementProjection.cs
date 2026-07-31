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
    /// Employer contributions and salary sacrifice for the year, after contributions tax.
    /// </summary>
    public decimal Contributions { get; init; }

    public decimal InvestmentReturn { get; init; }

    /// <summary>
    /// Administration fees and insurance premiums taken out during the year.
    /// </summary>
    public decimal Costs { get; init; }

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

    /// <summary>
    /// Taken out of the balances this year to fund the household's retirement income.
    /// </summary>
    public decimal Drawdown { get; init; }

    /// <summary>
    /// The same withdrawal expressed in today's dollars, which is the figure worth comparing across
    /// years — the nominal one grows with inflation even though its buying power does not.
    /// </summary>
    public decimal DrawdownInTodaysDollars { get; init; }

    /// <summary>
    /// The Age Pension the household is entitled to this year, given what it holds.
    /// </summary>
    public decimal Pension { get; init; }

    /// <summary>
    /// What the household actually lives on this year: the drawdown plus the pension.
    /// </summary>
    public decimal TotalIncome { get; init; }

    public decimal TotalIncomeInTodaysDollars { get; init; }

    /// <summary>
    /// Each member's part of the year, so a chart can show whose balance is funding the income.
    /// </summary>
    public IEnumerable<RetirementMemberYear> Members { get; init; } = [];
}

/// <summary>
/// One member's part of one projection year.
/// </summary>
public sealed record RetirementMemberYear
{
    public Guid MemberId { get; init; }

    public required string Name { get; init; }

    /// <summary>
    /// The age this member reaches in this year, which is what a retirement chart plots against.
    /// </summary>
    public int Age { get; init; }

    public decimal Contributions { get; init; }

    public decimal InvestmentReturn { get; init; }

    public decimal Costs { get; init; }

    /// <summary>
    /// Drawn from this member's balance this year — their share of the household's income.
    /// </summary>
    public decimal Drawdown { get; init; }

    public decimal ClosingBalance { get; init; }
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
    /// The level annual income, in today's dollars, that the household's balance at retirement would
    /// sustain to the plan's life expectancy. This is the figure a target income is solved against.
    /// </summary>
    /// <remarks>
    /// Computed on the household as a whole — its combined balance, the plan's return, and the years
    /// from the last retirement to life expectancy — rather than by summing the members' own figures.
    /// The two differ, because each member has their own return and their own retirement age, and the
    /// target income the sliders solve for is a household figure. Keeping this one on the same basis
    /// is what stops the page reporting a sustainable income that contradicts the target beside it.
    ///
    /// The per-member figures are still on each <see cref="RetirementMemberOutcome"/>.
    /// </remarks>
    public decimal AnnualRetirementIncomeInTodaysDollars { get; init; }

    /// <summary>
    /// The year the last member reaches their retirement age.
    /// </summary>
    public int RetirementYear { get; init; }

    /// <summary>
    /// The age of the last member to retire, which is where the drawdown starts.
    /// </summary>
    /// <remarks>
    /// Stated rather than left to be worked out from the years, because reconstructing it needs the
    /// life expectancy the projection was actually run under — and a caller holding unsaved slider
    /// values does not have it.
    /// </remarks>
    public int RetirementAge { get; init; }

    /// <summary>
    /// The real (above-inflation) return implied by the plan's return and inflation assumptions.
    /// This is the accumulation return, earned while the balances are still invested.
    /// </summary>
    public decimal RealReturnRate { get; init; }

    /// <summary>
    /// The real return earned once the balance has moved to cash, which is what it earns for the
    /// whole of retirement.
    /// </summary>
    /// <remarks>
    /// The rate that matters for how long the money lasts. A balance de-risked to cash before
    /// retirement does not go on earning the growth rate it earned while it was invested, and using
    /// the accumulation return to work out a sustainable income overstates it substantially — with a
    /// 6.5% growth assumption against a 3% cash rate and 2.5% inflation, by about a third.
    /// </remarks>
    public decimal DrawdownRealReturnRate { get; init; }

    /// <summary>
    /// Every fee and insurance premium taken out across the projection, so the drag they apply
    /// is visible rather than only implied by a smaller balance.
    /// </summary>
    public decimal TotalCosts { get; init; }

    /// <summary>
    /// What is left at life expectancy, after the whole drawdown. Nought means the plan spent it all.
    /// </summary>
    public decimal FinalBalance { get; init; }

    public decimal FinalBalanceInTodaysDollars { get; init; }

    /// <summary>
    /// The year the projection ends, being when the plan's life expectancy is reached.
    /// </summary>
    public int LifeExpectancyYear { get; init; }

    /// <summary>
    /// The first year the household could not draw its full target income, or null if the money
    /// lasted. This is the headline answer to whether the plan works.
    /// </summary>
    public int? MoneyRunsOutYear { get; init; }

    /// <summary>
    /// Everything the household draws from the Age Pension across the projection. Nought means it
    /// never qualified — usually because its balances stayed above the assets test's cut-off.
    /// </summary>
    public decimal TotalPension { get; init; }
}
