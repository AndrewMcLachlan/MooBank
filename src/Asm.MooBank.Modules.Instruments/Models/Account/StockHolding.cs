using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Account;

public static class StockHoldingExtensions
{
    public static InstrumentSummary ToModel(this Domain.Entities.StockHolding.StockHolding stockHolding, ICurrencyConverter currencyConverter) => new()
    {
        Id = stockHolding.Id,
        Name = stockHolding.Name,
        Description = stockHolding.Description,
        Controller = stockHolding.Controller,
        Currency = stockHolding.Currency,
        CurrentBalance = stockHolding.CurrentValue,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(stockHolding.CurrentValue, stockHolding.Currency),
        BalanceDate = ((Domain.Entities.Instrument.Instrument)stockHolding).LastUpdated,
        InstrumentType = "Shares",
    };

    public static IEnumerable<InstrumentSummary> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities, ICurrencyConverter currencyConverter) =>
        entities.Select(t => t.ToModel(currencyConverter));
}
