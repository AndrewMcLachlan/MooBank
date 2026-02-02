#nullable enable
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Bogus;

namespace Asm.MooBank.Modules.Assets.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static Asset CreateAsset(
        Guid? id = null,
        string? name = null,
        string? description = null,
        decimal value = 10000m,
        decimal? purchasePrice = null,
        string currency = "AUD",
        bool shareWithFamily = false,
        IEnumerable<InstrumentOwner>? owners = null)
    {
        var assetId = id ?? Guid.NewGuid();

        var asset = new Asset(assetId)
        {
            Name = name ?? Faker.Commerce.ProductName(),
            Description = description ?? Faker.Lorem.Sentence(),
            Value = value,
            PurchasePrice = purchasePrice,
            Currency = currency,
            ShareWithFamily = shareWithFamily,
            Controller = Controller.Manual,
        };

        if (owners != null)
        {
            foreach (var owner in owners)
            {
                asset.Owners.Add(owner);
            }
        }

        return asset;
    }

    public static InstrumentOwner CreateInstrumentOwner(
        Guid? userId = null,
        Guid? groupId = null)
    {
        var gId = groupId ?? Guid.NewGuid();
        return new InstrumentOwner
        {
            UserId = userId ?? Guid.NewGuid(),
            GroupId = groupId,
            Group = groupId.HasValue ? new Domain.Entities.Group.Group(gId) { Name = "Test Group" } : null,
        };
    }

    public static List<Asset> CreateSampleAssets(Guid? ownerId = null)
    {
        var owner = ownerId ?? Guid.NewGuid();

        return
        [
            CreateAsset(
                name: "House",
                description: "Primary residence",
                value: 750000m,
                purchasePrice: 500000m,
                owners: [CreateInstrumentOwner(owner)]),
            CreateAsset(
                name: "Car",
                description: "Family vehicle",
                value: 25000m,
                purchasePrice: 45000m,
                owners: [CreateInstrumentOwner(owner)]),
            CreateAsset(
                name: "Art Collection",
                description: "Various artwork",
                value: 15000m,
                purchasePrice: 8000m,
                owners: [CreateInstrumentOwner(owner)]),
        ];
    }

    public static IQueryable<Asset> CreateAssetQueryable(IEnumerable<Asset> assets)
    {
        return QueryableHelper.CreateAsyncQueryable(assets);
    }

    public static IQueryable<Asset> CreateAssetQueryable(params Asset[] assets)
    {
        return CreateAssetQueryable(assets.AsEnumerable());
    }
}
