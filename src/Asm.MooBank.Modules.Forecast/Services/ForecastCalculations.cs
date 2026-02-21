using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;

namespace Asm.MooBank.Modules.Forecast.Services;

internal sealed record RegressionModel(decimal Intercept, decimal Slope, decimal RSquared, bool Valid, decimal AvgHistoricalIncome);

/// <summary>
/// Pure computational logic for the forecast engine: regression fitting, baseline recalculation, and summary generation.
/// </summary>
internal static class ForecastCalculations
{
    /// <summary>
    /// Filters account IDs to exclude Savings accounts for historical analysis.
    /// Savings accounts often have large transfers that skew income/expense averages.
    /// </summary>
    public static List<Guid> FilterAccountsForHistoricalAnalysis(List<DomainInstrument> instruments) =>
        instruments
            .OfType<LogicalAccount>()
            .Where(a => a.AccountType != AccountType.Savings)
            .Select(a => a.Id)
            .ToList();

    /// <summary>
    /// Recalculates baseline outgoings using actual balance data from past months.
    /// For each past month where we have consecutive actual balances, we can derive
    /// what the actual outgoings were: actual_outgoings = opening + income + planned - closing.
    /// The average of these actuals replaces the historical baseline for the forecast.
    /// Falls back to the original baseline if no actual data is available.
    /// Note: uses predicted income since actual income data is not separately available.
    /// Inaccuracies in income prediction will be reflected in the derived outgoings.
    /// </summary>
    public static decimal RecalculateBaselineFromActuals(
        Dictionary<string, decimal?> actualBalancesByMonth,
        Dictionary<string, decimal> incomeByMonth,
        Dictionary<string, decimal> plannedItemsByMonth,
        DateOnly startDate, DateOnly endDate,
        decimal fallbackBaseline)
    {
        var actualOutgoings = new List<decimal>();
        var currentDate = new DateOnly(startDate.Year, startDate.Month, 1);
        var lastDate = new DateOnly(endDate.Year, endDate.Month, 1);

        while (currentDate <= lastDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            var nextMonthKey = currentDate.AddMonths(1).ToString("yyyy-MM");

            var opening = actualBalancesByMonth.GetValueOrDefault(monthKey);
            var closing = actualBalancesByMonth.GetValueOrDefault(nextMonthKey);

            if (opening.HasValue && closing.HasValue)
            {
                var income = incomeByMonth.GetValueOrDefault(monthKey, 0m);
                var planned = plannedItemsByMonth.GetValueOrDefault(monthKey, 0m);

                // Derive actual outgoings from balance change:
                // closing = opening + income - outgoings + planned
                // outgoings = opening + income + planned - closing
                var derived = opening.Value + income + planned - closing.Value;

                // Skip months where derived outgoings are negative — this indicates
                // unexplained balance growth (e.g. transfers in, windfalls) that would
                // distort the baseline average.
                if (derived >= 0)
                {
                    actualOutgoings.Add(derived);
                }
            }

            currentDate = currentDate.AddMonths(1);
        }

        return actualOutgoings.Count > 0 ? actualOutgoings.Average() : fallbackBaseline;
    }

    /// <summary>
    /// Fits a linear regression of expense = intercept + slope * income using aggregated monthly data.
    /// </summary>
    public static RegressionModel FitRegression(
        Dictionary<DateOnly, (decimal Income, decimal Expense)> monthlyData,
        IncomeCorrelatedSettings settings)
    {
        var points = monthlyData.Values.ToList();

        var avgIncome = points.Count > 0 ? points.Average(p => p.Income) : 0m;

        // Validate minimum data points
        if (points.Count < settings.MinDataPoints)
        {
            return new RegressionModel(0m, 0m, 0m, false, avgIncome);
        }

        // Fit simple linear regression: expense = intercept + slope * income
        var n = (decimal)points.Count;
        var sumX = points.Sum(p => p.Income);
        var sumY = points.Sum(p => p.Expense);
        var sumXY = points.Sum(p => p.Income * p.Expense);
        var sumXX = points.Sum(p => p.Income * p.Income);

        var denominator = n * sumXX - sumX * sumX;

        // Zero variance in income — cannot fit regression
        if (denominator == 0m)
        {
            return new RegressionModel(0m, 0m, 0m, false, avgIncome);
        }

        var slope = (n * sumXY - sumX * sumY) / denominator;
        var intercept = (sumY - slope * sumX) / n;

        // Compute R-squared
        var meanY = sumY / n;
        var ssTotal = points.Sum(p => (p.Expense - meanY) * (p.Expense - meanY));
        var ssResidual = points.Sum(p =>
        {
            var predicted = intercept + slope * p.Income;
            return (p.Expense - predicted) * (p.Expense - predicted);
        });

        var rSquared = ssTotal == 0m ? 0m : 1m - ssResidual / ssTotal;

        // Reject if R-squared below threshold or negative slope (nonsensical)
        var valid = rSquared >= settings.RSquaredThreshold && slope >= 0m;

        return new RegressionModel(intercept, slope, rSquared, valid, avgIncome);
    }

    public static ForecastSummary CalculateSummary(List<ForecastMonth> months, decimal monthlyBaselineOutgoings, RegressionModel? regression = null)
    {
        if (!months.Any())
        {
            return new ForecastSummary
            {
                LowestBalance = 0m,
                LowestBalanceMonth = DateOnly.FromDateTime(DateTime.Today),
                RequiredMonthlyUplift = 0m,
                MonthsBelowZero = 0,
                TotalIncome = 0m,
                TotalOutgoings = 0m,
                MonthlyBaselineOutgoings = 0m
            };
        }

        var lowestMonth = months.MinBy(m => m.ClosingBalance)!;
        var monthsUntilLow = months.TakeWhile(m => m != lowestMonth).Count() + 1;

        var requiredUplift = lowestMonth.ClosingBalance < 0 && monthsUntilLow > 0
            ? Math.Abs(lowestMonth.ClosingBalance) / monthsUntilLow
            : 0m;

        // Calculate total outgoings (baseline + planned expenses only, not planned income)
        var totalPlannedExpenses = months.Sum(m => m.PlannedItemsTotal < 0 ? Math.Abs(m.PlannedItemsTotal) : 0);
        var totalPlannedIncome = months.Sum(m => m.PlannedItemsTotal > 0 ? m.PlannedItemsTotal : 0);

        var effectiveBaseline = regression is { Valid: true }
            ? months.Average(m => Math.Abs(m.BaselineOutgoingsTotal))
            : monthlyBaselineOutgoings;

        return new ForecastSummary
        {
            LowestBalance = lowestMonth.ClosingBalance,
            LowestBalanceMonth = lowestMonth.MonthStart,
            RequiredMonthlyUplift = Math.Ceiling(requiredUplift * 100) / 100, // Round up to nearest cent
            MonthsBelowZero = months.Count(m => m.ClosingBalance < 0),
            TotalIncome = months.Sum(m => m.IncomeTotal) + totalPlannedIncome,
            TotalOutgoings = months.Sum(m => Math.Abs(m.BaselineOutgoingsTotal)) + totalPlannedExpenses,
            MonthlyBaselineOutgoings = effectiveBaseline,
            Regression = regression is not null ? new RegressionDiagnostics
            {
                FixedComponent = regression.Intercept,
                VariableComponent = regression.Slope,
                RSquared = regression.RSquared,
                FellBackToFlatAverage = !regression.Valid,
            } : null,
        };
    }
}
