using System.Text.Json;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
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

        // 1. Determine starting balance
        var startingBalance = await CalculateStartingBalance(plan, accountIds, cancellationToken);

        // 2. Parse strategies
        var incomeStrategy = String.IsNullOrEmpty(plan.IncomeStrategySerialized)
            ? new IncomeStrategy()
            : JsonSerializer.Deserialize<IncomeStrategy>(plan.IncomeStrategySerialized, JsonOptions)!;

        var outgoingStrategy = String.IsNullOrEmpty(plan.OutgoingStrategySerialized)
            ? new OutgoingStrategy()
            : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions)!;

        // 3. Calculate baseline outgoings from historical data
        var baselineOutgoings = await CalculateBaselineOutgoings(accountIds, outgoingStrategy, plan.StartDate, cancellationToken);

        // 4. Expand planned items into monthly allocations
        var plannedItemsByMonth = ExpandPlannedItems(plan);

        // 5. Calculate income by month (use historical if no manual amount or amount is 0)
        var incomeByMonth = await CalculateIncomeByMonth(plan, incomeStrategy, accountIds, cancellationToken);

        // 6. Generate forecast months
        var months = new List<ForecastMonth>();
        var currentBalance = startingBalance;
        var currentDate = new DateOnly(plan.StartDate.Year, plan.StartDate.Month, 1);
        var endDate = new DateOnly(plan.EndDate.Year, plan.EndDate.Month, 1);

        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            var monthIncome = incomeByMonth.GetValueOrDefault(monthKey, 0m);
            var monthPlanned = plannedItemsByMonth.GetValueOrDefault(monthKey, 0m);

            var forecastMonth = new ForecastMonth
            {
                MonthStart = currentDate,
                OpeningBalance = currentBalance,
                IncomeTotal = monthIncome,
                BaselineOutgoingsTotal = baselineOutgoings,
                PlannedItemsTotal = monthPlanned,
                ClosingBalance = currentBalance + monthIncome - (Math.Abs(baselineOutgoings) + Math.Abs(monthPlanned))
            };

            months.Add(forecastMonth);
            currentBalance = forecastMonth.ClosingBalance;
            currentDate = currentDate.AddMonths(1);
        }

        // 7. Calculate summary metrics
        var summary = CalculateSummary(months);

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
        if (plan.AccountScopeMode == DomainEntities.AccountScopeMode.SelectedAccounts)
        {
            return plan.Accounts.Select(a => a.InstrumentId).ToList();
        }

        // AllAccounts mode - use all user's accounts and shared accounts
        return user.Accounts.Concat(user.SharedAccounts).ToList();
    }

    private async Task<decimal> CalculateStartingBalance(DomainForecastPlan plan, List<Guid> accountIds, CancellationToken cancellationToken)
    {
        if (plan.StartingBalanceMode == DomainEntities.StartingBalanceMode.ManualAmount)
        {
            return plan.StartingBalanceAmount ?? 0m;
        }

        if (!accountIds.Any())
        {
            return 0m;
        }

        decimal totalBalance = 0m;
        foreach (var accountId in accountIds)
        {
            try
            {
                var instrument = await instrumentRepository.Get(accountId, cancellationToken);
                if (instrument is DomainTransactionInstrument transactionInstrument)
                {
                    totalBalance += transactionInstrument.Balance;
                }
            }
            catch (NotFoundException)
            {
                // Skip accounts that don't exist
            }
        }

        return totalBalance;
    }

    private async Task<decimal> CalculateBaselineOutgoings(List<Guid> accountIds, OutgoingStrategy strategy, DateOnly planStartDate, CancellationToken cancellationToken)
    {
        if (!accountIds.Any() || strategy.LookbackMonths <= 0)
        {
            return 0m;
        }

        var lookbackEnd = planStartDate.AddDays(-1);
        var lookbackStart = lookbackEnd.AddMonths(-strategy.LookbackMonths);

        decimal totalOutgoings = 0m;

        foreach (var accountId in accountIds)
        {
            try
            {
                var totals = await reportRepository.GetCreditDebitTotals(accountId, lookbackStart, lookbackEnd, cancellationToken);
                totalOutgoings += totals.Where(t => t.TransactionType == TransactionFilterType.Debit).Sum(t => t.Total);
            }
            catch (Exception)
            {
                // Skip accounts with errors
            }
        }

        // Calculate monthly average
        return totalOutgoings / strategy.LookbackMonths;
    }

    /// <summary>
    /// Calculates average monthly income from historical transaction data.
    /// </summary>
    private async Task<decimal> CalculateHistoricalIncome(List<Guid> accountIds, int lookbackMonths, DateOnly planStartDate, CancellationToken cancellationToken)
    {
        if (!accountIds.Any() || lookbackMonths <= 0)
        {
            return 0m;
        }

        var lookbackEnd = planStartDate.AddDays(-1);
        var lookbackStart = lookbackEnd.AddMonths(-lookbackMonths);

        decimal totalIncome = 0m;

        foreach (var accountId in accountIds)
        {
            try
            {
                var totals = await reportRepository.GetCreditDebitTotals(accountId, lookbackStart, lookbackEnd, cancellationToken);
                totalIncome += totals.Where(t => t.TransactionType == TransactionFilterType.Credit).Sum(t => t.Total);
            }
            catch (Exception)
            {
                // Skip accounts with errors
            }
        }

        // Calculate monthly average
        return totalIncome / lookbackMonths;
    }

    private Dictionary<string, decimal> ExpandPlannedItems(DomainForecastPlan plan)
    {
        var result = new Dictionary<string, decimal>();

        foreach (var item in plan.PlannedItems.Where(i => i.IsIncluded))
        {
            var sign = item.ItemType == DomainEntities.PlannedItemType.Income ? 1m : -1m;

            switch (item.DateMode)
            {
                case DomainEntities.PlannedItemDateMode.FixedDate when item.FixedDate.HasValue:
                    {
                        var monthKey = new DateOnly(item.FixedDate.Value.Year, item.FixedDate.Value.Month, 1).ToString("yyyy-MM");
                        if (item.FixedDate.Value >= plan.StartDate && item.FixedDate.Value <= plan.EndDate)
                        {
                            result[monthKey] = result.GetValueOrDefault(monthKey, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case DomainEntities.PlannedItemDateMode.Schedule when item.ScheduleAnchorDate.HasValue:
                    {
                        var occurrences = GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate);
                        foreach (var occurrence in occurrences)
                        {
                            var key = new DateOnly(occurrence.Year, occurrence.Month, 1).ToString("yyyy-MM");
                            result[key] = result.GetValueOrDefault(key, 0m) + (item.Amount * sign);
                        }
                        break;
                    }

                case DomainEntities.PlannedItemDateMode.FlexibleWindow when item.WindowStartDate.HasValue && item.WindowEndDate.HasValue:
                    {
                        var windowStart = item.WindowStartDate.Value < plan.StartDate ? plan.StartDate : item.WindowStartDate.Value;
                        var windowEnd = item.WindowEndDate.Value > plan.EndDate ? plan.EndDate : item.WindowEndDate.Value;

                        if (item.AllocationMode == DomainEntities.AllocationMode.AllAtEnd)
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

    private IEnumerable<DateOnly> GenerateScheduleOccurrences(DomainEntities.ForecastPlannedItem item, DateOnly planStart, DateOnly planEnd)
    {
        var occurrences = new List<DateOnly>();
        var current = item.ScheduleAnchorDate!.Value;
        var endDate = item.ScheduleEndDate ?? planEnd;
        if (endDate > planEnd) endDate = planEnd;

        while (current <= endDate)
        {
            if (current >= planStart)
            {
                occurrences.Add(current);
            }

            current = item.ScheduleFrequency switch
            {
                ScheduleFrequency.Daily => current.AddDays(item.ScheduleInterval ?? 1),
                ScheduleFrequency.Weekly => current.AddDays(7 * (item.ScheduleInterval ?? 1)),
                ScheduleFrequency.Monthly => AddMonthsWithDay(current, item.ScheduleInterval ?? 1, item.ScheduleDayOfMonth),
                ScheduleFrequency.Yearly => current.AddYears(item.ScheduleInterval ?? 1),
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

    private static ForecastSummary CalculateSummary(List<ForecastMonth> months)
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
                TotalOutgoings = 0m
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
            TotalIncome = months.Sum(m => m.IncomeTotal),
            TotalOutgoings = months.Sum(m => m.BaselineOutgoingsTotal + Math.Abs(m.PlannedItemsTotal))
        };
    }
}
