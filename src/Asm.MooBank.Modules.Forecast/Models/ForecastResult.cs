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
    public decimal PlannedItemsTotal { get; init; }
    public decimal ClosingBalance { get; init; }
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
}
