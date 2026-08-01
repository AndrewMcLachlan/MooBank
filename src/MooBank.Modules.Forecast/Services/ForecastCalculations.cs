using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// The span of whole calendar months the expense model is fitted over, inclusive at both ends.
/// </summary>
internal sealed record TrainingWindow(DateOnly StartMonth, DateOnly EndMonth)
{
    /// <summary>The last day of the final month, for querying by date range.</summary>
    public DateOnly EndDate => EndMonth.AddMonths(1).AddDays(-1);

    public bool Contains(DateOnly month) => month >= StartMonth && month <= EndMonth;

    public IEnumerable<DateOnly> Months()
    {
        for (var month = StartMonth; month <= EndMonth; month = month.AddMonths(1))
        {
            yield return month;
        }
    }
}

internal sealed record RegressionModel(decimal Intercept, decimal Slope, decimal RSquared, bool Valid, decimal AvgHistoricalIncome)
{
    /// <summary>
    /// No fit: too little data to attempt one. Consumers fall back to the flat average.
    /// </summary>
    public static RegressionModel None { get; } = new(0m, 0m, 0m, false, 0m);
}

/// <summary>
/// Pure computational logic for the forecast engine: regression fitting, baseline recalculation, and summary generation.
/// </summary>
internal static class ForecastCalculations
{
    /// <summary>
    /// Filters instruments to exclude Savings accounts for historical analysis.
    /// Savings accounts often have large transfers that skew income/expense averages.
    /// </summary>
    public static List<LogicalAccount> FilterInstrumentsForHistoricalAnalysis(List<DomainInstrument> instruments) =>
        instruments
            .OfType<LogicalAccount>()
            .Where(a => a.AccountType != AccountType.Savings)
            .ToList();

    /// <summary>
    /// Filters account IDs to exclude Savings accounts for historical analysis.
    /// </summary>
    public static List<Guid> FilterAccountsForHistoricalAnalysis(List<DomainInstrument> instruments) =>
        [.. FilterInstrumentsForHistoricalAnalysis(instruments).Select(a => a.Id)];

    /// <summary>
    /// The start of the last month the data covers in full, or null when it doesn't cover one.
    /// </summary>
    /// <remarks>
    /// A partial month holds a fraction of a month's income and spending. Fitting it as though it
    /// were a whole one puts a point near the origin, and a point near the origin anchors the
    /// regression line: in the real data a single day's tail — income $0, expenses $9 — pulled the
    /// fixed component from $6,965 down to $2,399, the slope from 0.327 to 0.529, and R² from 0.284
    /// to 0.691. Only whole months are fit.
    /// </remarks>
    public static DateOnly? LastCompleteMonth(DateOnly? dataThrough)
    {
        if (dataThrough is not { } through) return null;

        var monthStart = new DateOnly(through.Year, through.Month, 1);

        // The month is complete only if the data reaches its final day.
        return through >= monthStart.AddMonths(1).AddDays(-1) ? monthStart : monthStart.AddMonths(-1);
    }

    /// <summary>
    /// Builds the window of whole months the expense model is fitted over, or null when the data
    /// doesn't cover one.
    /// </summary>
    public static TrainingWindow? BuildTrainingWindow(DateOnly? dataThrough, int lookbackMonths)
    {
        if (lookbackMonths <= 0 || LastCompleteMonth(dataThrough) is not { } endMonth) return null;

        return new TrainingWindow(endMonth.AddMonths(-(lookbackMonths - 1)), endMonth);
    }

    /// <summary>
    /// Takes spending claimed by planned items back out of the pre-plan lookback average.
    /// </summary>
    /// <remarks>
    /// A yearly insurance bill or a term's school fees sits in the lookback window as ordinary
    /// spending while also being planned for the future, so without this it is charged to the
    /// forecast twice — once smeared across the baseline and again on its own date.
    /// </remarks>
    public static decimal ExcludeAttributedSpend(
        decimal baselineOutgoings,
        Dictionary<string, decimal> attributedByMonth,
        DateOnly planStartDate,
        int lookbackMonths)
    {
        if (lookbackMonths <= 0 || attributedByMonth.Count == 0) return baselineOutgoings;

        var lookbackEnd = new DateOnly(planStartDate.Year, planStartDate.Month, 1).AddDays(-1);
        var lookbackStart = new DateOnly(lookbackEnd.Year, lookbackEnd.Month, 1).AddMonths(-(lookbackMonths - 1));

        var attributed = attributedByMonth
            .Where(kvp => DateOnly.TryParseExact(kvp.Key, "yyyy-MM", out var month) && month >= lookbackStart && month <= lookbackEnd)
            .Sum(kvp => kvp.Value);

        return Math.Max(0m, baselineOutgoings - (attributed / lookbackMonths));
    }

    /// <summary>
    /// Recalculates baseline outgoings using actual balance data from past months.
    /// </summary>
    /// <param name="actualCreditsByMonth">Actual credits — every dollar that arrived, not modelled income.</param>
    /// <param name="plannedExpensesByMonth">Planned expenses allocated to each month, positive.</param>
    /// <remarks>
    /// Balances are the authority on what a month cost, so the total spent is read from the balance
    /// change rather than from the transaction feed:
    /// <code>
    /// closing   = opening + credits - spent
    /// spent     = opening + credits - closing
    /// baseline  = spent - plannedExpenses
    /// </code>
    /// The subtraction is what makes this a *baseline*: planned expenses are added to the forecast
    /// separately, so leaving them in here would count them twice.
    ///
    /// Planned income is deliberately absent. It is already part of <paramref name="actualCreditsByMonth"/>,
    /// and adding it again — as this did while a plan carried both a fixed income figure and planned
    /// income items — inflates every derived month by the planned income.
    /// </remarks>
    public static decimal RecalculateBaselineFromActuals(
        Dictionary<string, decimal?> actualBalancesByMonth,
        Dictionary<string, decimal> actualCreditsByMonth,
        Dictionary<string, decimal> plannedExpensesByMonth,
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
                var credits = actualCreditsByMonth.GetValueOrDefault(monthKey, 0m);
                var plannedExpenses = plannedExpensesByMonth.GetValueOrDefault(monthKey, 0m);

                var derived = opening.Value + credits - closing.Value - plannedExpenses;

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

        var effectiveBaseline = regression is { Valid: true }
            ? months.Average(m => Math.Abs(m.BaselineOutgoingsTotal))
            : monthlyBaselineOutgoings;

        return new ForecastSummary
        {
            LowestBalance = lowestMonth.ClosingBalance,
            LowestBalanceMonth = lowestMonth.MonthStart,
            RequiredMonthlyUplift = Math.Ceiling(requiredUplift * 100) / 100, // Round up to nearest cent
            MonthsBelowZero = months.Count(m => m.ClosingBalance < 0),
            // IncomeTotal is already the whole income model, so there is nothing to add to it.
            TotalIncome = months.Sum(m => m.IncomeTotal),
            TotalOutgoings = months.Sum(m => Math.Abs(m.BaselineOutgoingsTotal) + m.PlannedExpensesTotal),
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
