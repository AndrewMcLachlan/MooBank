using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public static class AssetExtensions
{
    public static async Task<InstrumentSummary> ToModel(this Domain.Entities.Asset.Asset asset, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) => new()
    {
        Id = asset.Id,
        Name = asset.Name,
        Description = asset.Description,
        Controller = asset.Controller,
        Currency = asset.Currency,
        CurrentBalance = asset.Value,
        CurrentBalanceLocalCurrency = await currencyConverter.Convert(asset.Value, asset.Currency, cancellationToken),
        BalanceDate = asset.LastUpdated,
        InstrumentType = "Asset",
    };

    public static async Task<IEnumerable<InstrumentSummary>> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) =>
        await entities.SelectAsync(entity => entity.ToModel(currencyConverter, cancellationToken));
}
