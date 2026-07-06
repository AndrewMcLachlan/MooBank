using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Assets.Models;

public record Asset : MooBank.Models.Instrument
{
    public decimal? PurchasePrice { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class AssetExtensions
{
    public static async Task<Asset> ToModel(this Domain.Entities.Asset.Asset asset, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default) => new()
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
        CurrentBalanceLocalCurrency = await currencyConverter.Convert(asset.Value, asset.Currency, cancellationToken),
        ShareWithFamily = asset.ShareWithFamily,
    };

    public static async Task<Asset> ToModel(this Domain.Entities.Asset.Asset asset, Guid userId, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        var result = await asset.ToModel(currencyConverter, cancellationToken);
        result.GroupId = asset.GetGroup(userId)?.Id;

        return result;
    }

    public static async Task<IEnumerable<Asset>> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities, ICurrencyConverter currencyConverter, CancellationToken cancellationToken = default)
    {
        List<Asset> models = [];

        foreach (var entity in entities)
        {
            models.Add(await entity.ToModel(currencyConverter, cancellationToken));
        }

        return models;
    }
}
