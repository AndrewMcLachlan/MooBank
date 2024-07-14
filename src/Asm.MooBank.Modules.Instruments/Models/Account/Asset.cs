using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Account;

public static class AssetExtensions
{
    public static InstrumentSummary ToModel(this Domain.Entities.Asset.Asset asset, ICurrencyConverter currencyConverter) => new()
    {
        Id = asset.Id,
        Name = asset.Name,
        Description = asset.Description,
        Controller = asset.Controller,
        Currency = asset.Currency,
        CurrentBalance = asset.Value,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(asset.Value, asset.Currency),
        BalanceDate = asset.LastUpdated,
        InstrumentType = "Asset",
    };

    public static IEnumerable<InstrumentSummary> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities, ICurrencyConverter currencyConverter) =>
        entities.Select(t => t.ToModel(currencyConverter));
}
