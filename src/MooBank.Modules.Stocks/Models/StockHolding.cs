using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Models;

public record StockHolding : MooBank.Models.Instrument
{
    public required string Symbol { get; init; }

    public int Quantity { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal? PreviousPrice { get; init; }

    public DateOnly? PreviousPriceDate { get; init; }

    public decimal CurrentValue { get; init; }

    public decimal GainLoss { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class StockHoldingExtensions
{
    public static async Task<StockHolding> ToModel(this Domain.Entities.StockHolding.StockHolding account, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Symbol = account.Symbol,
        Description = account.Description,
        Controller = account.Controller,
        CurrentBalance = account.CurrentValue,
        Currency = account.Currency,
        CurrentBalanceLocalCurrency = await currencyConverter.Convert(account.CurrentValue, account.Currency, cancellationToken),
        GainLoss = account.GainLoss,
        BalanceDate = ((Instrument)account).LastUpdated,
        InstrumentType = "Shares",
        CurrentPrice = account.CurrentPrice,
        Quantity = account.Quantity,
        CurrentValue = account.CurrentValue,
    };

    public static async Task<StockHolding> ToModel(this Domain.Entities.StockHolding.StockHolding account, Guid userId, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        var result = await account.ToModel(currencyConverter, cancellationToken);
        result.GroupId = account.GetGroup(userId)?.Id;

        return result;
    }

    public static async Task<IEnumerable<StockHolding>> ToModel(this IEnumerable<Domain.Entities.StockHolding.StockHolding> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) =>
        await entities.SelectAsync(entity => entity.ToModel(currencyConverter, cancellationToken));
}
