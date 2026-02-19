using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Assets.Models;

public record Asset : MooBank.Models.Instrument
{
    public decimal? PurchasePrice { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class AssetExtensions
{
    public static Asset ToModel(this Domain.Entities.Asset.Asset asset, ICurrencyConverter currencyConverter) => new()
    {
        Id = asset.Id,
        Name = asset.Name,
        Description = asset.Description,
        Controller = asset.Controller,
        CurrentBalance = asset.Value,
        BalanceDate = asset.LastUpdated,
        PurchasePrice = asset.PurchasePrice,
        InstrumentType = "Asset",
        Currency = asset.Currency,
        CurrentBalanceLocalCurrency = currencyConverter.Convert(asset.Value, asset.Currency),
        ShareWithFamily = asset.ShareWithFamily,
    };

    public static Asset ToModel(this Domain.Entities.Asset.Asset asset, Guid userId, ICurrencyConverter currencyConverter)
    {
        var result = asset.ToModel(currencyConverter);
        result.GroupId = asset.GetGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<Asset> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities, ICurrencyConverter currencyConverter)
    {
        return entities.Select(t => t.ToModel(currencyConverter));
    }
}
