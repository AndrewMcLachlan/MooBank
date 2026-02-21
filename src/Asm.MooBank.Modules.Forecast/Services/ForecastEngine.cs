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
    IReportRepository reportRepository,
    IInstrumentRepository instrumentRepository,
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

        // 4. Get account IDs excluding Savings accounts for historical calculations
        var accountIdsForHistoricalAnalysis = ForecastCalculations.FilterAccountsForHistoricalAnalysis(allInstruments);

        // 5. Parse strategies
        var incomeStrategy = String.IsNullOrEmpty(plan.IncomeStrategySerialized)
            ? new IncomeStrategy()
            : JsonSerializer.Deserialize<IncomeStrategy>(plan.IncomeStrategySerialized, JsonOptions)!;

        var outgoingStrategy = String.IsNullOrEmpty(plan.OutgoingStrategySerialized)
            ? new OutgoingStrategy()
            : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions)!;

        // 6. Determine starting balance (uses all selected accounts)
        var startingBalance = await CalculateStartingBalance(plan, allInstruments, accountIds, cancellationToken);

        // 7. Calculate baseline outgoings from historical data (excluding Savings accounts)
        var baselineOutgoings = await CalculateBaselineOutgoings(accountIdsForHistoricalAnalysis, outgoingStrategy, plan.StartDate, cancellationToken);

        // 8. Expand planned items into monthly allocations
        var plannedItemsByMonth = PlannedItemExpander.ExpandPlannedItems(plan);

        // 9. Calculate income by month (excluding Savings accounts for historical calculation)
        var (incomeByMonth, planBaseIncome) = await CalculateIncomeByMonth(plan, incomeStrategy, accountIdsForHistoricalAnalysis, cancellationToken);

        // 10. Fetch historical actual balances for comparison
        var actualBalancesByMonth = await GetActualBalancesByMonth(accountIds, plan.StartDate, plan.EndDate, latestTransactionDate, cancellationToken);

        // 11. Fit income-expense regression if mode is IncomeCorrelated
        RegressionModel? regressionModel = null;
        var useRegression = false;
        var regressionIncomeOffset = 0m;

        if (outgoingStrategy.Mode == "IncomeCorrelated")
        {
            regressionModel = await FitIncomeExpenseRegression(
                accountIdsForHistoricalAnalysis, outgoingStrategy, latestTransactionDate, cancellationToken);

            useRegression = regressionModel.Valid;

            if (useRegression)
            {
                // Offset between the regression's training income (total credits) and the plan's
                // income (typically salary only). Ensures the regression input is on the same scale.
                regressionIncomeOffset = regressionModel.AvgHistoricalIncome - planBaseIncome;
            }
            // If regression is invalid, baselineOutgoings from step 7 is used as-is
        }

        // 12. Recalculate baseline outgoings from actual balance data if available
        //     (skip when using valid regression — outgoings vary per month)
        decimal effectiveBaselineOutgoings;
        if (useRegression)
        {
            effectiveBaselineOutgoings = baselineOutgoings; // unused per-month, but kept for summary fallback
        }
        else
        {
            effectiveBaselineOutgoings = ForecastCalculations.RecalculateBaselineFromActuals(
                actualBalancesByMonth, incomeByMonth, plannedItemsByMonth,
                plan.StartDate, plan.EndDate, baselineOutgoings);
        }

        // 13. Generate forecast months
        var months = new List<ForecastMonth>();
        var currentBalance = startingBalance;
        var currentDate = new DateOnly(plan.StartDate.Year, plan.StartDate.Month, 1);
        var endDate = new DateOnly(plan.EndDate.Year, plan.EndDate.Month, 1);

        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            var monthIncome = incomeByMonth.GetValueOrDefault(monthKey, 0m);
            var monthPlanned = plannedItemsByMonth.GetValueOrDefault(monthKey, 0m);
            var actualBalance = actualBalancesByMonth.GetValueOrDefault(monthKey);

            var monthOutgoings = useRegression
                ? Math.Max(0m, regressionModel!.Intercept + regressionModel.Slope * (monthIncome + regressionIncomeOffset))
                : effectiveBaselineOutgoings;

            var forecastMonth = new ForecastMonth
            {
                MonthStart = currentDate,
                OpeningBalance = currentBalance,
                IncomeTotal = monthIncome,
                BaselineOutgoingsTotal = monthOutgoings,
                PlannedItemsTotal = monthPlanned,
                // monthPlanned already has correct sign: positive for income, negative for expenses
                ClosingBalance = currentBalance + monthIncome - Math.Abs(monthOutgoings) + monthPlanned,
                ActualBalance = actualBalance
            };

            months.Add(forecastMonth);
            currentBalance = forecastMonth.ClosingBalance;
            currentDate = currentDate.AddMonths(1);
        }

        // 14. Calculate summary metrics
        var summary = ForecastCalculations.CalculateSummary(months, effectiveBaselineOutgoings, regressionModel);

        return new ForecastResult
        {
            PlanId = plan.Id,
            Months = months,
            Summary = summary
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
        var allBalances = await reportRepository.GetMonthlyBalancesForAccounts(accountIds, previousMonth, previousMonthEnd, cancellationToken);

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
        var allTotals = await reportRepository.GetCreditDebitTotalsForAccounts(accountIds, lookbackStart, lookbackEnd, cancellationToken);

        var totalOutgoings = allTotals.Values
            .SelectMany(t => t)
            .Where(t => t.TransactionType == TransactionFilterType.Debit)
            .Sum(t => t.Total);

        // Calculate monthly average
        return totalOutgoings / strategy.LookbackMonths;
    }

    /// <summary>
    /// Calculates average monthly income from historical transaction data.
    /// </summary>
    private async Task<decimal> CalculateHistoricalIncome(List<Guid> accountIds, int lookbackMonths, DateOnly planStartDate, CancellationToken cancellationToken)
    {
        if (accountIds.Count == 0 || lookbackMonths <= 0)
        {
            return 0m;
        }

        var lookbackEnd = planStartDate.AddDays(-1);
        var lookbackStart = lookbackEnd.AddMonths(-lookbackMonths);

        // Batch query all accounts in parallel
        var allTotals = await reportRepository.GetCreditDebitTotalsForAccounts(accountIds, lookbackStart, lookbackEnd, cancellationToken);

        var totalIncome = allTotals.Values
            .SelectMany(t => t)
            .Where(t => t.TransactionType == TransactionFilterType.Credit)
            .Sum(t => t.Total);

        // Calculate monthly average
        return totalIncome / lookbackMonths;
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
        var allBalances = await reportRepository.GetMonthlyBalancesForAccounts(accountIds, fetchStart, effectiveEndDate, cancellationToken);

        var startMonth = new DateOnly(startDate.Year, startDate.Month, 1);
        var endMonth = new DateOnly(effectiveEndDate.Year, effectiveEndDate.Month, 1);

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

    private async Task<(Dictionary<string, decimal> ByMonth, decimal BaseIncome)> CalculateIncomeByMonth(DomainForecastPlan plan, IncomeStrategy strategy, List<Guid> accountIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, decimal>();

        var currentDate = new DateOnly(plan.StartDate.Year, plan.StartDate.Month, 1);
        var endDate = new DateOnly(plan.EndDate.Year, plan.EndDate.Month, 1);

        // Determine the base monthly income amount
        decimal baseMonthlyIncome;

        if (strategy.Mode == "ManualRecurring" && strategy.ManualRecurring != null && strategy.ManualRecurring.Amount > 0)
        {
            // Use manually specified amount
            baseMonthlyIncome = strategy.ManualRecurring.Amount;
        }
        else
        {
            // Calculate from historical data (default to 12 months lookback)
            var lookbackMonths = strategy.Historical?.LookbackMonths ?? 12;
            baseMonthlyIncome = await CalculateHistoricalIncome(accountIds, lookbackMonths, plan.StartDate, cancellationToken);
        }

        // Get date range for income (from manual settings or plan dates)
        var incomeStart = strategy.ManualRecurring?.StartDate ?? plan.StartDate;
        var incomeEnd = strategy.ManualRecurring?.EndDate ?? plan.EndDate;

        // Build adjustments dictionary
        var adjustments = (strategy.ManualAdjustments ?? []).ToDictionary(a => a.Date.ToString("yyyy-MM"), a => a.DeltaAmount);
        var currentAmount = baseMonthlyIncome;

        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");

            // Apply any adjustments (adjustments are cumulative - once applied, they persist)
            if (adjustments.TryGetValue(monthKey, out var adjustment))
            {
                currentAmount += adjustment;
            }

            if (currentDate >= new DateOnly(incomeStart.Year, incomeStart.Month, 1) &&
                currentDate <= new DateOnly(incomeEnd.Year, incomeEnd.Month, 1))
            {
                result[monthKey] = currentAmount;
            }

            currentDate = currentDate.AddMonths(1);
        }

        return (result, baseMonthlyIncome);
    }

    /// <summary>
    /// Fetches monthly credit/debit data and fits a linear regression of expense vs income.
    /// </summary>
    private async Task<RegressionModel> FitIncomeExpenseRegression(
        List<Guid> accountIds, OutgoingStrategy strategy, DateOnly latestTransactionDate, CancellationToken cancellationToken)
    {
        var lookbackEnd = latestTransactionDate;
        var lookbackStart = lookbackEnd.AddMonths(-strategy.LookbackMonths);

        var allTotals = await reportRepository.GetMonthlyCreditDebitTotalsForAccounts(accountIds, lookbackStart, lookbackEnd, cancellationToken);

        var monthlyData = AggregateMonthlyData(allTotals);
        var settings = strategy.IncomeCorrelated ?? new IncomeCorrelatedSettings();

        return ForecastCalculations.FitRegression(monthlyData, settings);
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
