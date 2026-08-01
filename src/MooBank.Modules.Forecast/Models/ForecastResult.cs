using System.ComponentModel;

namespace Asm.MooBank.Modules.Forecast.Models;

[DisplayName("ForecastResult")]
public sealed record ForecastResult
{
    public Guid PlanId { get; init; }
    public required IEnumerable<ForecastMonth> Months { get; init; }
    public required ForecastSummary Summary { get; init; }

    /// <summary>
    /// How each planned item is tracking against what was actually spent.
    /// </summary>
    public IEnumerable<PlannedItemProgress> PlannedItems { get; init; } = [];
}

/// <summary>
/// A planned item measured against the spending that carried its tag.
/// </summary>
/// <remarks>
/// This is how the author knows to adjust the plan, which makes it as much the feature as the
/// arithmetic is: the engine reports the divergence, and correcting it is theirs to do.
/// </remarks>
[DisplayName("PlannedItemProgress")]
public sealed record PlannedItemProgress
{
    public Guid PlannedItemId { get; init; }

    public required string Name { get; init; }

    /// <summary>What the item expects to cost in total across the plan.</summary>
    public decimal PlannedTotal { get; init; }

    /// <summary>What has actually been spent against it so far.</summary>
    public decimal ActualToDate { get; init; }

    public decimal Remaining { get; init; }

    /// <summary>
    /// Whether the item carries a tag. Without one nothing can be attributed to it, and it simply
    /// stands as planned.
    /// </summary>
    public bool IsMatched { get; init; }

    /// <summary>
    /// Whether the window in which spending could still be attributed has passed. A closed item with
    /// nothing against it never happened; one with less than it planned came in under.
    /// </summary>
    public bool IsClosed { get; init; }
}

[DisplayName("ForecastMonth")]
public sealed record ForecastMonth
{
    public DateOnly MonthStart { get; init; }
    public decimal OpeningBalance { get; init; }
    /// <summary>
    /// Income allocated to this month, entirely from planned income items.
    /// </summary>
    public decimal IncomeTotal { get; init; }
    public decimal BaselineOutgoingsTotal { get; init; }
    /// <summary>
    /// Planned expenses allocated to this month (positive).
    /// </summary>
    public decimal PlannedExpensesTotal { get; init; }
    /// <summary>
    /// Of <see cref="PlannedExpensesTotal"/>, how much is actual spending attributed to a planned
    /// item rather than a figure still only planned. Nought for months yet to happen.
    /// </summary>
    public decimal RealisedExpensesTotal { get; init; }
    public decimal ClosingBalance { get; init; }
    /// <summary>
    /// The actual historical balance for this month, if available (null for future months).
    /// </summary>
    public decimal? ActualBalance { get; init; }
    /// <summary>
    /// Actual income (total credits) for this month, if historical (null for future months).
    /// Scoped to the historical-analysis accounts (excludes savings), matching <see cref="IncomeTotal"/>.
    /// </summary>
    public decimal? ActualIncome { get; init; }
    /// <summary>
    /// Actual outgoings (total debits, positive) for this month, if historical (null for future months).
    /// Scoped to the historical-analysis accounts (excludes savings), matching <see cref="BaselineOutgoingsTotal"/>.
    /// </summary>
    public decimal? ActualOutgoings { get; init; }
}

[DisplayName("ForecastSummary")]
public sealed record ForecastSummary
{
    public decimal LowestBalance { get; init; }
    public DateOnly LowestBalanceMonth { get; init; }
    public decimal RequiredMonthlyUplift { get; init; }
    public int MonthsBelowZero { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalOutgoings { get; init; }
    public decimal MonthlyBaselineOutgoings { get; init; }
    public RegressionDiagnostics? Regression { get; init; }
}

[DisplayName("RegressionDiagnostics")]
public sealed record RegressionDiagnostics
{
    public decimal FixedComponent { get; init; }
    public decimal VariableComponent { get; init; }
    public decimal RSquared { get; init; }
    public bool FellBackToFlatAverage { get; init; }
}
