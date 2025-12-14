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
public record CreatePlan([FromBody]Models.ForecastPlan Plan) : ICommand<Models.ForecastPlan>;

internal class CreatePlanHandler(
    IForecastRepository forecastRepository,
    IReportRepository reportRepository,
    IInstrumentRepository instrumentRepository,
    IUnitOfWork unitOfWork,
    User user) : ICommandHandler<CreatePlan, Models.ForecastPlan>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const int DefaultLookbackMonths = 12;

    public async ValueTask<Models.ForecastPlan> Handle(CreatePlan request, CancellationToken cancellationToken)
    {
        // Determine which accounts to use for calculations
        var accountIds = request.Plan.AccountIds.Any()
            ? request.Plan.AccountIds.ToList()
            : user.Accounts.Concat(user.SharedAccounts).ToList();

        // Pre-calculate historical values if not provided
        var incomeStrategy = request.Plan.IncomeStrategy ?? new IncomeStrategy();
        var outgoingStrategy = request.Plan.OutgoingStrategy ?? new OutgoingStrategy();

        // Calculate starting balance if using calculated mode
        decimal? startingBalance = request.Plan.StartingBalanceAmount;
        if (request.Plan.StartingBalanceMode == Models.StartingBalanceMode.CalculatedCurrent && !startingBalance.HasValue)
        {
            startingBalance = await CalculateCurrentBalance(accountIds, cancellationToken);
        }

        // Calculate historical income if not manually specified
        if (incomeStrategy.ManualRecurring == null || incomeStrategy.ManualRecurring.Amount == 0)
        {
            var historicalIncome = await CalculateHistoricalAverage(accountIds, request.Plan.StartDate, DefaultLookbackMonths, TransactionFilterType.Credit, cancellationToken);
            incomeStrategy = incomeStrategy with
            {
                Mode = "ManualRecurring",
                ManualRecurring = new ManualRecurringIncome { Amount = historicalIncome, Frequency = "Monthly" }
            };
        }

        // Calculate historical outgoings if using default lookback
        if (outgoingStrategy.LookbackMonths == 0)
        {
            var historicalOutgoings = await CalculateHistoricalAverage(accountIds, request.Plan.StartDate, DefaultLookbackMonths, TransactionFilterType.Debit, cancellationToken);
            outgoingStrategy = outgoingStrategy with
            {
                LookbackMonths = DefaultLookbackMonths,
                Mode = "HistoricalAverage"
            };
        }

        var entity = new Domain.Entities.Forecast.ForecastPlan(Guid.NewGuid())
        {
            FamilyId = user.FamilyId,
            Name = request.Plan.Name,
            StartDate = request.Plan.StartDate,
            EndDate = request.Plan.EndDate,
            AccountScopeMode = (Domain.Entities.Forecast.AccountScopeMode)request.Plan.AccountScopeMode,
            StartingBalanceMode = (Domain.Entities.Forecast.StartingBalanceMode)request.Plan.StartingBalanceMode,
            StartingBalanceAmount = startingBalance,
            CurrencyCode = request.Plan.CurrencyCode ?? user.Currency,
            IncomeStrategySerialized = JsonSerializer.Serialize(incomeStrategy, JsonOptions),
            OutgoingStrategySerialized = JsonSerializer.Serialize(outgoingStrategy, JsonOptions),
            AssumptionsSerialized = request.Plan.Assumptions != null ? JsonSerializer.Serialize(request.Plan.Assumptions, JsonOptions) : null,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        if (request.Plan.AccountIds.Any())
        {
            entity.SetAccounts(request.Plan.AccountIds);
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
                var totals = await reportRepository.GetCreditDebitTotals(accountId, lookbackStart, lookbackEnd, cancellationToken);
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
