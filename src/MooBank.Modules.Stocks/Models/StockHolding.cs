using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Models;

public record StockHolding : MooBank.Models.Instrument
{
    public required string Symbol { get; init; }

    public int Quantity { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal CurrentValue { get; init; }

    public decimal GainLoss { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class StockHoldingExtensions
{
    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account, ICurrencyConverter currencyConverter) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Symbol = account.Symbol,
        Description = account.Description,
        Controller = account.Controller,
        CurrentBalance = account.CurrentValue,
        Currency = account.Currency,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(account.CurrentValue, account.Currency),
        GainLoss = account.GainLoss,
        BalanceDate = ((Instrument)account).LastUpdated,
        InstrumentType = "Shares",
        CurrentPrice = account.CurrentPrice,
        Quantity = account.Quantity,
        CurrentValue = account.CurrentValue,
    };

    public static StockHolding ToModel(this Domain.Entities.StockHolding.StockHolding account, Guid userId, ICurrencyConverter currencyConverter)
    {
        var result = account.ToModel(currencyConverter);
        result.GroupId = account.GetGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<StockHolding> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities, ICurrencyConverter currencyConverter)
    {
        return entities.Select(t => t.ToModel(currencyConverter));
    }
}
