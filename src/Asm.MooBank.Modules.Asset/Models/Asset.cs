namespace Asm.MooBank.Modules.Assets.Models;
public record Asset : Account
{
    public decimal? PurchasePrice { get; init; }

    public bool ShareWithFamily { get; init; }
}

public static class AssetExtensions
{
    public static Asset ToModel(this Domain.Entities.Asset.Asset account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Description = account.Description,
        CurrentBalance = account.Balance,
        BalanceDate = account.LastUpdated,
        PurchasePrice = account.PurchasePrice,
        AccountType = "Asset",
    };

    public static Asset ToModel(this Domain.Entities.Asset.Asset asset, Guid userId)
    {
        var result = asset.ToModel();
        result.AccountGroupId = asset.GetAccountGroup(userId)?.Id;

        return result;
    }

    public static IEnumerable<Asset> ToModel(this IEnumerable<Domain.Entities.Asset.Asset> entities)
    {
        return entities.Select(t => t.ToModel());
    }
}
