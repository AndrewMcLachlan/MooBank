using System.ComponentModel;

namespace Asm.MooBank.Modules.Forecast.Models;

[DisplayName("ForecastResult")]
public sealed record ForecastResult
{
    public Guid PlanId { get; init; }
    public required IEnumerable<ForecastMonth> Months { get; init; }
    public required ForecastSummary Summary { get; init; }
}

[DisplayName("ForecastMonth")]
public sealed record ForecastMonth
{
    public DateOnly MonthStart { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal IncomeTotal { get; init; }
    public decimal BaselineOutgoingsTotal { get; init; }
    /// <summary>
    /// Net planned items for this month: positive for income, negative for expenses.
    /// </summary>
    public decimal PlannedItemsTotal { get; init; }
    /// <summary>
    /// Planned income allocated to this month (positive). Split out of <see cref="PlannedItemsTotal"/>
    /// so income and expenses can be charted independently.
    /// </summary>
    public decimal PlannedIncomeTotal { get; init; }
    /// <summary>
    /// Planned expenses allocated to this month (positive). Split out of <see cref="PlannedItemsTotal"/>
    /// so income and expenses can be charted independently.
    /// </summary>
    public decimal PlannedExpensesTotal { get; init; }
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
