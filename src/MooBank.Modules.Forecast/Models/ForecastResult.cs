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

    /// <summary>
    /// How ordinary spending is modelled.
    /// </summary>
    public required ExpenseModel Expenses { get; init; }
}

/// <summary>
/// How ordinary monthly spending is modelled: a fixed amount, plus a share of whatever is earned.
/// </summary>
/// <remarks>
/// There is no single monthly expenses figure, and reporting one would be a fiction. Spending moves
/// with income — a lower income means less discretionary spending — so the honest answer has two
/// parts, and only two parts can say what happens when income changes.
///
/// The fit is always used. Household spending is noisy enough that a real relationship rarely
/// reaches a high correlation over a year or two of monthly points, and rejecting it on that basis
/// left the forecast on a flat line that could not answer the question it exists for.
/// <see cref="RSquared"/> says how much of the variation it accounts for; where that is low the
/// variable part is small, which is the honest version of the same caution.
/// </remarks>
[DisplayName("ExpenseModel")]
public sealed record ExpenseModel
{
    /// <summary>What is spent each month regardless of income.</summary>
    public decimal FixedComponent { get; init; }

    /// <summary>The share of each additional dollar earned that gets spent, as a rate.</summary>
    public decimal VariableComponent { get; init; }

    /// <summary>How much of the variation in spending the fit accounts for.</summary>
    public decimal RSquared { get; init; }

    /// <summary>How many whole months the fit was made from.</summary>
    public int DataPoints { get; init; }

    /// <summary>
    /// How far the plan's modelled income falls short of the credits actually seen each month.
    /// </summary>
    /// <remarks>
    /// Nought is healthy. A large figure means the plan is not modelling all of its income, and
    /// spending is being priced at a higher income than the plan credits itself with — so it is
    /// worth showing rather than quietly correcting for.
    /// </remarks>
    public decimal ModelledIncomeShortfall { get; init; }

    /// <summary>
    /// What the model works out to per month on average across the plan. A convenience, not the
    /// model: it cannot be applied to a month whose income differs.
    /// </summary>
    public decimal AverageMonthly { get; init; }
}
