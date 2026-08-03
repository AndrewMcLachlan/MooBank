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

internal sealed record RegressionModel(decimal Intercept, decimal Slope, decimal RSquared, bool Valid, decimal AvgHistoricalIncome, int DataPoints)
{
    /// <summary>
    /// No fit: too little data to attempt one. Consumers fall back to the flat average.
    /// </summary>
    public static RegressionModel None { get; } = new(0m, 0m, 0m, false, 0m, 0);
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
    /// Average ordinary monthly spending: what was actually spent, less whatever a planned item
    /// claims.
    /// </summary>
    /// <param name="actualByMonth">Actual income and spending per month, spending positive.</param>
    /// <param name="attributedByMonth">Spending already claimed by a planned item.</param>
    /// <remarks>
    /// Read from what was spent rather than inferred from the change in balance. Balances are raw —
    /// they have to be, since they are the account's actual position — while the spending totals
    /// honour the exclusions that decide what counts. Subtracting one from the other therefore made
    /// every excluded transaction look like an unexplained difference, and unexplained differences
    /// landed in "spending": a $20,000 transfer between two of the plan's own accounts, which never
    /// left the pool at all, read as $20,000 spent.
    ///
    /// Planned spending comes out because the forecast adds it back on its own date. What remains is
    /// the ordinary spending a baseline is meant to describe, on the same footing as the data the
    /// expense model is fitted to.
    /// </remarks>
    public static decimal AverageOrdinarySpending(
        Dictionary<string, (decimal Income, decimal Expense)> actualByMonth,
        Dictionary<string, decimal> attributedByMonth,
        DateOnly startDate,
        DateOnly throughMonth,
        decimal fallback)
    {
        var months = new List<decimal>();
        var current = new DateOnly(startDate.Year, startDate.Month, 1);
        var last = new DateOnly(throughMonth.Year, throughMonth.Month, 1);

        while (current <= last)
        {
            var monthKey = current.ToString("yyyy-MM");

            // Only months the data actually covers. A month with no transactions is a gap in the
            // record, not a month where nothing was spent, and averaging it in would drag the
            // baseline down.
            if (actualByMonth.TryGetValue(monthKey, out var actual))
            {
                months.Add(Math.Max(0m, actual.Expense - attributedByMonth.GetValueOrDefault(monthKey, 0m)));
            }

            current = current.AddMonths(1);
        }

        return months.Count > 0 ? months.Average() : fallback;
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

        // Too little to fit a slope through. The caller supplies a level instead.
        if (points.Count < settings.MinDataPoints)
        {
            return new RegressionModel(0m, 0m, 0m, false, avgIncome, points.Count);
        }

        // Fit simple linear regression: expense = intercept + slope * income
        var n = (decimal)points.Count;
        var sumX = points.Sum(p => p.Income);
        var sumY = points.Sum(p => p.Expense);
        var sumXY = points.Sum(p => p.Income * p.Expense);
        var sumXX = points.Sum(p => p.Income * p.Income);

        var denominator = n * sumXX - sumX * sumX;

        // Every month earned the same, so there is no slope to find.
        if (denominator == 0m)
        {
            return new RegressionModel(0m, 0m, 0m, false, avgIncome, points.Count);
        }

        var slope = (n * sumXY - sumX * sumY) / denominator;
        var intercept = (sumY - slope * sumX) / n;

        var meanY = sumY / n;
        var ssTotal = points.Sum(p => (p.Expense - meanY) * (p.Expense - meanY));
        var ssResidual = points.Sum(p =>
        {
            var predicted = intercept + slope * p.Income;
            return (p.Expense - predicted) * (p.Expense - predicted);
        });

        var rSquared = ssTotal == 0m ? 0m : 1m - ssResidual / ssTotal;

        // The fit is used whatever its correlation. What is not allowed is a nonsensical shape: a
        // negative slope has spending fall as income rises, and a slope above one has every extra
        // dollar earned spent more than once over, which is a countdown rather than a forecast.
        // Those are clamped back to the nearest believable line through the data rather than thrown
        // away, because the alternative -- a flat average -- cannot answer what happens when income
        // changes, and answering that is the point.
        var clamped = Math.Clamp(slope, 0m, 1m);

        if (clamped != slope)
        {
            intercept = meanY - (clamped * (sumX / n));
            slope = clamped;
        }

        return new RegressionModel(intercept, slope, rSquared, true, avgIncome, points.Count);
    }

    public static ForecastSummary CalculateSummary(
        List<ForecastMonth> months,
        decimal levelWithoutAFit,
        RegressionModel regression,
        decimal modelledIncomeShortfall)
    {
        var expenses = new ExpenseModel
        {
            // Without enough months to find a slope, spending is modelled as a level with no
            // variable part -- reported as such rather than as a separate kind of answer.
            FixedComponent = regression.Valid ? regression.Intercept : levelWithoutAFit,
            VariableComponent = regression.Valid ? regression.Slope : 0m,
            RSquared = regression.RSquared,
            DataPoints = regression.DataPoints,
            ModelledIncomeShortfall = modelledIncomeShortfall,
            AverageMonthly = months.Count > 0 ? months.Average(m => Math.Abs(m.BaselineOutgoingsTotal)) : 0m,
        };

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
                Expenses = expenses,
            };
        }

        var lowestMonth = months.MinBy(m => m.ClosingBalance)!;
        var monthsUntilLow = months.TakeWhile(m => m != lowestMonth).Count() + 1;

        var requiredUplift = lowestMonth.ClosingBalance < 0 && monthsUntilLow > 0
            ? Math.Abs(lowestMonth.ClosingBalance) / monthsUntilLow
            : 0m;

        return new ForecastSummary
        {
            LowestBalance = lowestMonth.ClosingBalance,
            LowestBalanceMonth = lowestMonth.MonthStart,
            RequiredMonthlyUplift = Math.Ceiling(requiredUplift * 100) / 100, // Round up to nearest cent
            MonthsBelowZero = months.Count(m => m.ClosingBalance < 0),
            // IncomeTotal is already the whole income model, so there is nothing to add to it.
            TotalIncome = months.Sum(m => m.IncomeTotal),
            TotalOutgoings = months.Sum(m => Math.Abs(m.BaselineOutgoingsTotal) + m.PlannedExpensesTotal),
            Expenses = expenses,
        };
    }
}
