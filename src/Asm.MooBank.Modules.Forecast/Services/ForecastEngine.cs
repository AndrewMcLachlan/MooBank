using System.Text.Json;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainForecastPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;
using DomainLogicalAccount = Asm.MooBank.Domain.Entities.Account.LogicalAccount;
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
        // Resolve account IDs based on scope mode
        var accountIds = GetAccountIds(plan);

        // Pre-load all instruments in a single query to avoid N+1
        var allInstruments = (await instrumentRepository.Get(accountIds, cancellationToken)).ToList();

        // 2. Get account IDs excluding Savings accounts for historical calculations
        var accountIdsForHistoricalAnalysis = FilterAccountsForHistoricalAnalysis(allInstruments);

        // 3. Parse strategies
        var incomeStrategy = String.IsNullOrEmpty(plan.IncomeStrategySerialized)
            ? new IncomeStrategy()
            : JsonSerializer.Deserialize<IncomeStrategy>(plan.IncomeStrategySerialized, JsonOptions)!;

        var outgoingStrategy = String.IsNullOrEmpty(plan.OutgoingStrategySerialized)
            ? new OutgoingStrategy()
            : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions)!;

        // 1. Determine starting balance (uses all selected accounts)
        var startingBalance = await CalculateStartingBalance(plan, allInstruments, accountIds, cancellationToken);

        // 4. Calculate baseline outgoings from historical data (excluding Savings accounts)
        var baselineOutgoings = await CalculateBaselineOutgoings(accountIdsForHistoricalAnalysis, outgoingStrategy, plan.StartDate, cancellationToken);

        // 5. Expand planned items into monthly allocations
        var plannedItemsByMonth = ExpandPlannedItems(plan);

        // 6. Calculate income by month (excluding Savings accounts for historical calculation)
        var incomeByMonth = await CalculateIncomeByMonth(plan, incomeStrategy, accountIdsForHistoricalAnalysis, cancellationToken);

        // 7. Fetch historical actual balances for comparison
        var actualBalancesByMonth = await GetActualBalancesByMonth(accountIds, plan.StartDate, plan.EndDate, cancellationToken);

        // 8. Recalculate baseline outgoings from actual balance data if available
        var effectiveBaselineOutgoings = RecalculateBaselineFromActuals(
            actualBalancesByMonth, incomeByMonth, plannedItemsByMonth,
            plan.StartDate, plan.EndDate, baselineOutgoings);

        // 9. Generate forecast months
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

            var forecastMonth = new ForecastMonth
            {
                MonthStart = currentDate,
                OpeningBalance = currentBalance,
                IncomeTotal = monthIncome,
                BaselineOutgoingsTotal = effectiveBaselineOutgoings,
                PlannedItemsTotal = monthPlanned,
                // monthPlanned already has correct sign: positive for income, negative for expenses
                ClosingBalance = currentBalance + monthIncome - Math.Abs(effectiveBaselineOutgoings) + monthPlanned,
                ActualBalance = actualBalance
            };

            months.Add(forecastMonth);
            currentBalance = forecastMonth.ClosingBalance;
            currentDate = currentDate.AddMonths(1);
        }

        // 10. Calculate summary metrics
        var summary = CalculateSummary(months, effectiveBaselineOutgoings);

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

    /// <summary>
    /// Filters account IDs to exclude Savings accounts for historical analysis.
    /// Savings accounts often have large transfers that skew income/expense averages.
    /// </summary>
    private static List<Guid> FilterAccountsForHistoricalAnalysis(List<DomainInstrument> instruments) =>
        instruments
            .OfType<DomainLogicalAccount>()
            .Where(a => a.AccountType != AccountType.Savings)
            .Select(a => a.Id)
            .ToList();

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
    private async Task<Dictionary<string, decimal?>> GetActualBalancesByMonth(List<Guid> accountIds, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, decimal?>();

        if (accountIds.Count == 0)
        {
            return result;
        }

        // Only fetch balances up to current month
        var today = DateOnly.FromDateTime(DateTime.Today);
        var effectiveEndDate = endDate > today ? today : endDate;

        // If start date is in the future, no actual balances to fetch
        if (startDate > today)
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

    /// <summary>
    /// Recalculates baseline outgoings using actual balance data from past months.
    /// For each past month where we have consecutive actual balances, we can derive
    /// what the actual outgoings were: actual_outgoings = opening + income + planned - closing.
    /// The average of these actuals replaces the historical baseline for the forecast.
    /// Falls back to the original baseline if no actual data is available.
    /// Note: uses predicted income since actual income data is not separately available.
    /// Inaccuracies in income prediction will be reflected in the derived outgoings.
    /// </summary>
    private static decimal RecalculateBaselineFromActuals(
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

    private Dictionary<string, decimal> ExpandPlannedItems(DomainForecastPlan plan)
    {
        var result = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var sign = item.ItemType == PlannedItemType.Income ? 1m : -1m;

            switch (item.DateMode)
            {
                case PlannedItemDateMode.FixedDate when item.FixedDate != null:
                    {
                        var fixedDate = item.FixedDate.FixedDate;
                        var monthKey = new DateOnly(fixedDate.Year, fixedDate.Month, 1).ToString("yyyy-MM");
                        if (fixedDate >= plan.StartDate && fixedDate <= plan.EndDate)
                        {
                            result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case PlannedItemDateMode.Schedule when item.Schedule != null:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            result[key] = result.GetValueOrDefault(key, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case PlannedItemDateMode.FlexibleWindow when item.FlexibleWindow != null:
                    {
                        var windowStart = item.FlexibleWindow.StartDate < plan.StartDate ? plan.StartDate : item.FlexibleWindow.StartDate;
                        var windowEnd = item.FlexibleWindow.EndDate > plan.EndDate ? plan.EndDate : item.FlexibleWindow.EndDate;

                        if (item.FlexibleWindow.AllocationMode == AllocationMode.AllAtEnd)
                        {
                            var endKey = new DateOnly(windowEnd.Year, windowEnd.Month, 1).ToString("yyyy-MM");
                            result[endKey] = result.GetValueOrDefault(endKey, 0m) + (item.Amount * sign);
                        }
                        else // EvenlySpread
                        {
                            var months = CountMonths(windowStart, windowEnd);
                            if (months > 0)
                            {
                                var amountPerMonth = item.Amount / months;
                                var current = new DateOnly(windowStart.Year, windowStart.Month, 1);
                                var end = new DateOnly(windowEnd.Year, windowEnd.Month, 1);
                                while (current <= end)
                                {
                                    var key = current.ToString("yyyy-MM");
                                    result[key] = result.GetValueOrDefault(key, 0m) + (amountPerMonth * sign);
                                    current = current.AddMonths(1);
                                }
                            }
                        }
                        break;
                    }
            }
        }

        return result;
    }

    private static int CountMonths(DateOnly start, DateOnly end)
    {
        return ((end.Year - start.Year) * 12) + end.Month - start.Month + 1;
    }

    private IEnumerable<DateOnly> GenerateScheduleOccurrences(DomainForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var occurrences = new List<DateOnly>();
        var schedule = item.Schedule!;
        var current = schedule.AnchorDate;
        var endDate = schedule.EndDate ?? planEnd;
        if (endDate > planEnd) endDate = planEnd;

        while (current <= endDate)
        {
            if (current >= planStart)
            {
                occurrences.Add(current);
            }

            current = schedule.Frequency switch
            {
                ScheduleFrequency.Daily => current.AddDays(schedule.Interval),
                ScheduleFrequency.Weekly => current.AddDays(7 * schedule.Interval),
                ScheduleFrequency.Fortnightly => current.AddDays(14 * schedule.Interval),
                ScheduleFrequency.Monthly => AddMonthsWithDay(current, schedule.Interval, schedule.DayOfMonth),
                ScheduleFrequency.Yearly => current.AddYears(schedule.Interval),
                _ => current.AddMonths(1)
            };
        }

        return occurrences;
    }

    private static DateOnly AddMonthsWithDay(DateOnly date, int months, int? dayOfMonth)
    {
        var newDate = date.AddMonths(months);
        if (dayOfMonth.HasValue)
        {
            var maxDay = DateTime.DaysInMonth(newDate.Year, newDate.Month);
            var day = Math.Min(dayOfMonth.Value, maxDay);
            newDate = new DateOnly(newDate.Year, newDate.Month, day);
        }
        return newDate;
    }

    private async Task<Dictionary<string, decimal>> CalculateIncomeByMonth(DomainForecastPlan plan, IncomeStrategy strategy, List<Guid> accountIds, CancellationToken cancellationToken)
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

        return result;
    }

    private static ForecastSummary CalculateSummary(List<ForecastMonth> months, decimal monthlyBaselineOutgoings)
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

        return new ForecastSummary
        {
            LowestBalance = lowestMonth.ClosingBalance,
            LowestBalanceMonth = lowestMonth.MonthStart,
            RequiredMonthlyUplift = Math.Ceiling(requiredUplift * 100) / 100, // Round up to nearest cent
            MonthsBelowZero = months.Count(m => m.ClosingBalance < 0),
            TotalIncome = months.Sum(m => m.IncomeTotal) + totalPlannedIncome,
            TotalOutgoings = months.Sum(m => Math.Abs(m.BaselineOutgoingsTotal)) + totalPlannedExpenses,
            MonthlyBaselineOutgoings = monthlyBaselineOutgoings
        };
    }
}
