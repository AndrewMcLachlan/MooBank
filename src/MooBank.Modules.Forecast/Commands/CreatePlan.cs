using System.ComponentModel;
using System.Text.Json;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("CreateForecastPlan")]
public record CreatePlan([FromBody] Models.ForecastPlan Plan) : ICommand<Models.ForecastPlan>;

internal class CreatePlanHandler(
    IForecastRepository forecastRepository,
    IReportReader reportReader,
    IInstrumentRepository instrumentRepository,
    IUnitOfWork unitOfWork,
    User user) : ICommandHandler<CreatePlan, Models.ForecastPlan>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    /// <summary>
    /// How far back the expense model is fitted over by default. Two years rather than one: the fit
    /// needs enough months, and enough variation in income, to find a slope at all.
    /// </summary>
    private const int DefaultLookbackMonths = 24;

    /// <summary>
    /// How far back the seeded income item is averaged over. Deliberately shorter than the expense
    /// lookback — income tends to rise, so a two-year average would seed a figure the household has
    /// already grown out of.
    /// </summary>
    private const int IncomeSeedMonths = 12;

    public async ValueTask<Models.ForecastPlan> Handle(CreatePlan request, CancellationToken cancellationToken)
    {
        // Determine which accounts to use for calculations
        var accountIds = request.Plan.AccountIds.Any()
            ? request.Plan.AccountIds
            : user.Accounts.Concat(user.SharedAccounts);

        // Pre-calculate historical values if not provided
        var outgoingStrategy = request.Plan.OutgoingStrategy ?? new OutgoingStrategy();

        // Calculate starting balance if using calculated mode
        decimal? startingBalance = request.Plan.StartingBalanceAmount;
        if (request.Plan.StartingBalanceMode == StartingBalanceMode.CalculatedCurrent && !startingBalance.HasValue)
        {
            startingBalance = await CalculateCurrentBalance(accountIds, cancellationToken);
        }

        // Calculate historical outgoings if using default lookback
        if (outgoingStrategy.LookbackMonths == 0)
        {
            outgoingStrategy = outgoingStrategy with
            {
                LookbackMonths = DefaultLookbackMonths,
            };
        }

        var entity = new Domain.Entities.Forecast.ForecastPlan(Guid.NewGuid())
        {
            FamilyId = user.FamilyId,
            Name = request.Plan.Name,
            StartDate = request.Plan.StartDate,
            EndDate = request.Plan.EndDate,
            AccountScopeMode = request.Plan.AccountScopeMode,
            StartingBalanceMode = request.Plan.StartingBalanceMode,
            StartingBalanceAmount = startingBalance,
            CurrencyCode = request.Plan.CurrencyCode ?? user.Currency,
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            AssumptionsSerialized = request.Plan.Assumptions != null ? JsonSerializer.Serialize(request.Plan.Assumptions, JsonOptions) : null,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        if (request.Plan.AccountIds.Any())
        {
            entity.SetAccounts(request.Plan.AccountIds);
        }

        // Seed income from history so a new plan forecasts something on its first run. This used to
        // be a fixed monthly figure on the plan; it is now an ordinary planned income item, which
        // the author can date, end or split as their circumstances change.
        if (!request.Plan.PlannedItems.Any(i => i.ItemType == PlannedItemType.Income))
        {
            var historicalIncome = await CalculateHistoricalAverage(accountIds, request.Plan.StartDate, IncomeSeedMonths, TransactionFilterType.Credit, cancellationToken);

            if (historicalIncome > 0m)
            {
                entity.AddPlannedItem(new ForecastPlannedItem
                {
                    Name = "Income",
                    ItemType = PlannedItemType.Income,
                    Amount = historicalIncome,
                    IsIncluded = true,
                    DateMode = PlannedItemDateMode.Schedule,
                    Schedule = new PlannedItemSchedule
                    {
                        Frequency = ScheduleFrequency.Monthly,
                        AnchorDate = request.Plan.StartDate,
                        Interval = 1,
                    },
                });
            }
        }

        forecastRepository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    private async Task<decimal> CalculateCurrentBalance(IEnumerable<Guid> accountIds, CancellationToken cancellationToken)
    {
        decimal totalBalance = 0m;
        foreach (var accountId in accountIds)
        {
            try
            {
                var instrument = await instrumentRepository.Get(accountId, cancellationToken);
                if (instrument is Domain.Entities.Instrument.TransactionInstrument transactionInstrument)
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

    private async Task<decimal> CalculateHistoricalAverage(IEnumerable<Guid> accountIds, DateOnly planStartDate, int lookbackMonths, TransactionFilterType transactionType, CancellationToken cancellationToken)
    {
        if (!accountIds.Any() || lookbackMonths <= 0)
        {
            return 0m;
        }

        var lookbackEnd = planStartDate.AddDays(-1);
        var lookbackStart = lookbackEnd.AddMonths(-lookbackMonths);

        decimal total = 0m;
        foreach (var accountId in accountIds)
        {
            try
            {
                var totals = await reportReader.GetCreditDebitTotals(accountId, lookbackStart, lookbackEnd, cancellationToken);
                total += totals.Where(t => t.TransactionType == transactionType).Sum(t => t.Total);
            }
            catch (Exception)
            {
                // Skip accounts with errors
            }
        }

        return Math.Round(total / lookbackMonths, 2);
    }
}
