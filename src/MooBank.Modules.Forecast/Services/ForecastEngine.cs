using System.Text.Json;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainTransactionInstrument = Asm.MooBank.Domain.Entities.Instrument.TransactionInstrument;

namespace Asm.MooBank.Modules.Forecast.Services;

internal class ForecastEngine(
    IReportReader reportReader,
    IInstrumentRepository instrumentRepository,
    IPlannedItemMatcher plannedItemMatcher,
    User user) : IForecastEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<ForecastResult> Calculate(DomainForecastPlan plan, CancellationToken cancellationToken = default)
    {
        // 1. Resolve account IDs based on scope mode
        var accountIds = GetAccountIds(plan);

        // 2. Pre-load all instruments in a single query to avoid N+1
        var allInstruments = (await instrumentRepository.Get(accountIds, cancellationToken)).ToList();

        // 3. Determine the latest transaction date across all accounts (data freshness boundary)
        var latestTransactionDate = allInstruments
            .OfType<DomainTransactionInstrument>()
            .Select(i => i.LastTransaction)
            .Where(d => d.HasValue)
            .Max() ?? DateOnly.FromDateTime(DateTime.Today);

        // 4. Get accounts excluding Savings for historical calculations
        var instrumentsForHistoricalAnalysis = ForecastCalculations.FilterInstrumentsForHistoricalAnalysis(allInstruments);
        var accountIdsForHistoricalAnalysis = instrumentsForHistoricalAnalysis.Select(a => a.Id).ToList();

        // 4a. How far the data behind the *historical* figures runs. This is deliberately not
        //     latestTransactionDate: that is the maximum across every account, so a savings account
        //     with fresher data would extend the training window past the end of the transaction
        //     accounts being fit, adding an empty month at the far end.
        var historicalDataThrough = instrumentsForHistoricalAnalysis
            .Select(i => i.LastTransaction)
            .Where(d => d.HasValue)
            .Max() ?? latestTransactionDate;

        // 5. Parse the outgoing strategy. There is no income strategy: income comes from planned
        //    income items and nowhere else.
        var outgoingStrategy = String.IsNullOrEmpty(plan.OutgoingStrategySerialized)
            ? new OutgoingStrategy()
            : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions)!;

        // 6. Determine starting balance (uses all selected accounts)
        var startingBalance = await CalculateStartingBalance(plan, allInstruments, accountIds, cancellationToken);

        // 7. Calculate baseline outgoings from historical data (excluding Savings accounts)
        var baselineOutgoings = await CalculateBaselineOutgoings(accountIdsForHistoricalAnalysis, outgoingStrategy, plan.StartDate, cancellationToken);

        // 8. Expand planned items into monthly allocations, then measure them against the spending
        //    that actually carried their tags. Income and expenses stay apart: income is the plan's
        //    whole income model, and expenses are added on top of the baseline.
        var latestTransactionMonth = new DateOnly(latestTransactionDate.Year, latestTransactionDate.Month, 1);
        var realised = await RealisePlannedItems(
            plan, accountIdsForHistoricalAnalysis, latestTransactionMonth, cancellationToken);

        var incomeByMonth = realised.IncomeByMonth;
        var plannedExpensesByMonth = realised.ExpensesByMonth;

        // 9. Spending a planned item claims is that item's, never baseline. Taken out of the
        //    pre-plan lookback average here, and out of the regression's training data below.
        baselineOutgoings = ForecastCalculations.ExcludeAttributedSpend(
            baselineOutgoings, realised.AttributedByMonth, plan.StartDate, outgoingStrategy.LookbackMonths);

        // 10. Fetch historical actual balances for comparison
        var actualBalancesByMonth = await GetActualBalancesByMonth(accountIds, plan.StartDate, plan.EndDate, latestTransactionDate, cancellationToken);

        // 10a. Fetch actual monthly income/expenses (excl. savings, matching the projected series)
        //      for the projected-vs-actual income and expenses charts.
        var actualIncomeExpenseByMonth = await GetActualMonthlyIncomeAndExpenses(
            accountIdsForHistoricalAnalysis, plan.StartDate, latestTransactionDate, cancellationToken);

        // 11. Fit the expense model: spending as a fixed amount plus a share of income. Always
        //     attempted — it is the model, not a mode — and falls back to a flat average only when
        //     there is too little signal to fit a slope.
        var trainingWindow = ForecastCalculations.BuildTrainingWindow(historicalDataThrough, outgoingStrategy.LookbackMonths);

        var regressionModel = await FitIncomeExpenseRegression(
                accountIdsForHistoricalAnalysis, outgoingStrategy, trainingWindow, realised.AttributedByMonth, cancellationToken);

        var useRegression = regressionModel.Valid;
        var modelledIncomeShortfall = useRegression && trainingWindow is not null
            ? ModelledIncomeShortfall(regressionModel.AvgHistoricalIncome, incomeByMonth, trainingWindow)
            : 0m;

        // 12. Where there were too few months to find a slope, spending is modelled as a level with
        //     no variable part. Same accounts, same exclusions and same figures the fit would have
        //     used, so it describes the same thing the fit describes -- just without the slope.
        var levelWithoutAFit = useRegression ? 0m : ForecastCalculations.AverageOrdinarySpending(
            actualIncomeExpenseByMonth, realised.AttributedByMonth,
            plan.StartDate, latestTransactionMonth, baselineOutgoings);

        // 13. Generate forecast months
        var months = new List<ForecastMonth>();
        var currentBalance = startingBalance;
        var currentDate = new DateOnly(plan.StartDate.Year, plan.StartDate.Month, 1);
        var endDate = new DateOnly(plan.EndDate.Year, plan.EndDate.Month, 1);

        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            var monthIncome = incomeByMonth.GetValueOrDefault(monthKey, 0m);
            var monthPlannedExpenses = plannedExpensesByMonth.GetValueOrDefault(monthKey, 0m);
            var actualBalance = actualBalancesByMonth.GetValueOrDefault(monthKey);

            var monthOutgoings = useRegression
                ? Math.Max(0m, regressionModel.Intercept + regressionModel.Slope * (monthIncome + modelledIncomeShortfall))
                : levelWithoutAFit;

            // Actual income/expenses only exist for historical months (up to the latest transaction).
            var isHistorical = currentDate <= latestTransactionMonth;
            var actual = isHistorical ? actualIncomeExpenseByMonth.GetValueOrDefault(monthKey) : ((decimal Income, decimal Expense)?)null;

            var forecastMonth = new ForecastMonth
            {
                MonthStart = currentDate,
                OpeningBalance = currentBalance,
                IncomeTotal = monthIncome,
                BaselineOutgoingsTotal = monthOutgoings,
                PlannedExpensesTotal = monthPlannedExpenses,
                RealisedExpensesTotal = realised.AttributedByMonth.GetValueOrDefault(monthKey, 0m),
                ClosingBalance = currentBalance + monthIncome - Math.Abs(monthOutgoings) - monthPlannedExpenses,
                ActualBalance = actualBalance,
                ActualIncome = actual?.Income,
                ActualOutgoings = actual?.Expense
            };

            months.Add(forecastMonth);
            currentBalance = forecastMonth.ClosingBalance;
            currentDate = currentDate.AddMonths(1);
        }

        // 14. Calculate summary metrics
        var summary = ForecastCalculations.CalculateSummary(months, levelWithoutAFit, regressionModel, modelledIncomeShortfall);

        return new ForecastResult
        {
            PlanId = plan.Id,
            Months = months,
            Summary = summary,
            PlannedItems = realised.Progress,
        };
    }

    /// <summary>
    /// Gets the list of account IDs to use for calculations based on the plan's scope mode.
    /// </summary>
    private List<Guid> GetAccountIds(DomainForecastPlan plan)
    {
        if (plan.AccountScopeMode == AccountScopeMode.SelectedAccounts)
        {
            return [.. plan.Accounts.Select(a => a.InstrumentId)];
        }

        // AllAccounts mode - use all user's accounts and shared accounts
        return [.. user.Accounts, .. user.SharedAccounts];
    }

    private async Task<decimal> CalculateStartingBalance(DomainForecastPlan plan, List<DomainInstrument> instruments, List<Guid> accountIds, CancellationToken cancellationToken)
    {
        if (plan.StartingBalanceMode == StartingBalanceMode.ManualAmount)
        {
            return plan.StartingBalanceAmount ?? 0m;
        }

        if (instruments.Count == 0)
        {
            return 0m;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var planStartMonth = new DateOnly(plan.StartDate.Year, plan.StartDate.Month, 1);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        // If plan starts in the past, use historical balance from that month (batch query)
        if (planStartMonth < currentMonth)
        {
            return await CalculateHistoricalStartingBalance(accountIds, plan.StartDate, cancellationToken);
        }

        // Plan starts this month or in the future - use current balances from pre-loaded instruments
        return instruments
            .OfType<DomainTransactionInstrument>()
            .Sum(i => i.Balance);
    }

    private async Task<decimal> CalculateHistoricalStartingBalance(List<Guid> accountIds, DateOnly startDate, CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0)
        {
            return 0m;
        }

        // Get the closing balance of the month before the start date
        // This becomes the opening balance for the start month
        var previousMonth = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(-1);
        var previousMonthEnd = previousMonth.AddMonths(1).AddDays(-1);

        // Batch query all accounts in parallel
        var allBalances = await reportReader.GetMonthlyBalancesForAccounts(accountIds, previousMonth, previousMonthEnd, cancellationToken);

        return allBalances.Values
            .SelectMany(b => b)
            .Sum(b => b.Balance);
    }

    private async Task<decimal> CalculateBaselineOutgoings(List<Guid> accountIds, OutgoingStrategy strategy, DateOnly planStartDate, CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0 || strategy.LookbackMonths <= 0)
        {
            return 0m;
        }

        var lookbackEnd = planStartDate.AddDays(-1);
        var lookbackStart = lookbackEnd.AddMonths(-strategy.LookbackMonths);

        // Batch query all accounts in parallel
        var allTotals = await reportReader.GetCreditDebitTotalsForAccounts(accountIds, lookbackStart, lookbackEnd, cancellationToken);

        var totalOutgoings = allTotals.Values
            .SelectMany(t => t)
            .Where(t => t.TransactionType == TransactionFilterType.Debit)
            .Sum(t => t.Total);

        // Calculate monthly average
        return totalOutgoings / strategy.LookbackMonths;
    }

    /// <summary>
    /// Measures the plan's items against the spending that actually carried their tags.
    /// </summary>
    /// <remarks>
    /// Only the payments the author has linked. A tag identifies a category rather than a project,
    /// so it cannot say which payment belongs to which item; it is used to narrow what is offered
    /// when linking, and plays no part here.
    /// </remarks>
    private async Task<RealisedPlan> RealisePlannedItems(
        DomainForecastPlan plan,
        List<Guid> historicalAccountIds,
        DateOnly latestTransactionMonth,
        CancellationToken cancellationToken)
    {
        var included = plan.PlannedItems.Where(i => i.IsIncluded).ToList();

        var linkedTransactionIds = included
            .SelectMany(i => i.Transactions.Select(t => t.TransactionId))
            .Distinct()
            .ToList();

        var payments = await plannedItemMatcher.GetPayments(linkedTransactionIds, cancellationToken);

        return PlannedItemRealiser.Realise(plan, payments, historicalAccountIds, latestTransactionMonth);
    }

    /// <summary>
    /// How far the plan's modelled income falls short of the credits actually seen, averaged over
    /// the training months. Added to the modelled income before the regression reads it.
    /// </summary>
    /// <remarks>
    /// The regression is fitted against total credits — salary, but also refunds, interest and
    /// cashback — while planned income items list only what the author chose to model. Feeding
    /// modelled income straight in would read expenses off the wrong point on the line.
    ///
    /// A well-modelled plan drives this to nought on its own, which is the point of computing it
    /// against the income series rather than a single figure: when it is large it is reporting that
    /// the income model is missing something, and it is surfaced so the outlook can say so. The
    /// figure it replaced was the gap to one flat annual salary, which for a plan whose history
    /// included income the plan didn't model was permanently and invisibly large — it had the
    /// forecast spending at a high-income level while earning at a low-income one.
    ///
    /// Only months the plan models income for are counted; with no overlap the shortfall is nought.
    /// </remarks>
    private static decimal ModelledIncomeShortfall(decimal avgHistoricalIncome, Dictionary<string, decimal> incomeByMonth, TrainingWindow window)
    {
        var modelled = window.Months()
            .Select(month => incomeByMonth.TryGetValue(month.ToString("yyyy-MM"), out var income) ? income : (decimal?)null)
            .Where(income => income.HasValue)
            .Select(income => income!.Value)
            .ToList();

        return modelled.Count == 0 ? 0m : avgHistoricalIncome - modelled.Average();
    }

    /// <summary>
    /// Gets actual historical opening balances for each month.
    /// The opening balance for a month is the closing balance of the previous month.
    /// Returns a dictionary keyed by month (yyyy-MM) with aggregated balances.
    /// Only includes months up to the current month.
    /// </summary>
    private async Task<Dictionary<string, decimal?>> GetActualBalancesByMonth(List<Guid> accountIds, DateOnly startDate, DateOnly endDate, DateOnly latestTransactionDate, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, decimal?>();

        if (accountIds.Count == 0)
        {
            return result;
        }

        // Only fetch balances up to the latest transaction date
        var effectiveEndDate = endDate > latestTransactionDate ? latestTransactionDate : endDate;

        // If start date is beyond latest data, no actual balances to fetch
        if (startDate > latestTransactionDate)
        {
            return result;
        }

        // Fetch from the month before start date to get opening balance for start month
        var fetchStart = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(-1);

        // Batch query all accounts in parallel
        var allBalances = await reportReader.GetMonthlyBalancesForAccounts(accountIds, fetchStart, effectiveEndDate, cancellationToken);

        var startMonth = new DateOnly(startDate.Year, startDate.Month, 1);
        var endMonth = new DateOnly(effectiveEndDate.Year, effectiveEndDate.Month, 1).AddMonths(1);

        foreach (var (_, balances) in allBalances)
        {
            foreach (var balance in balances)
            {
                // The closing balance of this month becomes the opening balance of next month
                var nextMonth = new DateOnly(balance.PeriodEnd.Year, balance.PeriodEnd.Month, 1).AddMonths(1);
                var monthKey = nextMonth.ToString("yyyy-MM");

                // Only include if within our forecast range
                if (nextMonth >= startMonth && nextMonth <= endMonth)
                {
                    result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + balance.Balance;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Fetches monthly credit/debit data and fits a linear regression of expense vs income.
    /// </summary>
    /// <remarks>
    /// The window covers whole calendar months only. Anchoring it on the last transaction instead
    /// would open and close it mid-month, and a part-month reads to the fit as a month where almost
    /// nothing was earned or spent.
    /// </remarks>
    private async Task<RegressionModel> FitIncomeExpenseRegression(
        List<Guid> accountIds, OutgoingStrategy strategy, TrainingWindow? window,
        Dictionary<string, decimal> attributedByMonth, CancellationToken cancellationToken)
    {
        if (window is null) return RegressionModel.None;

        var allTotals = await reportReader.GetMonthlyCreditDebitTotalsForAccounts(accountIds, window.StartMonth, window.EndDate, cancellationToken);

        // Filtered here as well as in the query, so the whole-month guarantee holds locally rather
        // than resting on how the stored procedure treats the range boundaries.
        var monthlyData = AggregateMonthlyData(allTotals)
            .Where(kvp => window.Contains(kvp.Key))
            .ToDictionary(
                kvp => kvp.Key,
                // Spending a planned item claims is not ordinary spending, and the model being fitted
                // is one of ordinary spending. Leaving a solar installation in the training data
                // teaches the fit that a household at that income spends that much every month.
                kvp => (kvp.Value.Income,
                        Expense: Math.Max(0m, kvp.Value.Expense - attributedByMonth.GetValueOrDefault(kvp.Key.ToString("yyyy-MM"), 0m))));

        return ForecastCalculations.FitRegression(monthlyData, strategy.IncomeCorrelated ?? new IncomeCorrelatedSettings());
    }

    /// <summary>
    /// Gets actual monthly income (credits) and outgoings (debits, positive) for the given range,
    /// keyed by month (yyyy-MM). Feeds the projected-vs-actual income and expenses charts.
    /// </summary>
    private async Task<Dictionary<string, (decimal Income, decimal Expense)>> GetActualMonthlyIncomeAndExpenses(
        List<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, (decimal Income, decimal Expense)>();

        if (accountIds.Count == 0 || startDate > endDate)
        {
            return result;
        }

        var allTotals = await reportReader.GetMonthlyCreditDebitTotalsForAccounts(accountIds, startDate, endDate, cancellationToken);

        foreach (var (month, values) in AggregateMonthlyData(allTotals))
        {
            result[month.ToString("yyyy-MM")] = values;
        }

        return result;
    }

    /// <summary>
    /// Aggregates monthly credit/debit totals across multiple accounts into per-month (income, expense) pairs.
    /// </summary>
    private static Dictionary<DateOnly, (decimal Income, decimal Expense)> AggregateMonthlyData(
        Dictionary<Guid, IEnumerable<MonthlyCreditDebitTotal>> allTotals)
    {
        var monthlyData = new Dictionary<DateOnly, (decimal Income, decimal Expense)>();

        foreach (var totals in allTotals.Values)
        {
            foreach (var total in totals)
            {
                if (!monthlyData.TryGetValue(total.Month, out var existing))
                {
                    existing = (0m, 0m);
                }

                if (total.TransactionType == TransactionFilterType.Credit)
                {
                    monthlyData[total.Month] = (existing.Income + total.Total, existing.Expense);
                }
                else if (total.TransactionType == TransactionFilterType.Debit)
                {
                    // Debit totals come back negative from the SP; use Abs so expenses are positive for regression
                    monthlyData[total.Month] = (existing.Income, existing.Expense + Math.Abs(total.Total));
                }
            }
        }

        return monthlyData;
    }
}
