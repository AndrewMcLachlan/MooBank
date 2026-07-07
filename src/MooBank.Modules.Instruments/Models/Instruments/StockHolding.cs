using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public static class StockHoldingExtensions
{
    public static async Task<InstrumentSummary> ToModel(this Domain.Entities.StockHolding.StockHolding stockHolding, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) => new()
    {
        Id = stockHolding.Id,
        Name = stockHolding.Name,
        Description = stockHolding.Description,
        Controller = stockHolding.Controller,
        Currency = stockHolding.Currency,
        CurrentBalance = stockHolding.CurrentValue,
        CurrentBalanceLocalCurrency = await currencyConverter.Convert(stockHolding.CurrentValue, stockHolding.Currency, cancellationToken),
        BalanceDate = ((Domain.Entities.Instrument.Instrument)stockHolding).LastUpdated,
        InstrumentType = "Shares",
    };

    public static async Task<IEnumerable<InstrumentSummary>> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) =>
        await entities.SelectAsync(entity => entity.ToModel(currencyConverter, cancellationToken));
}
