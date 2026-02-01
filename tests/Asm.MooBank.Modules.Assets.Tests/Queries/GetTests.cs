#nullable enable
using Asm.MooBank.Modules.Assets.Queries;
using Asm.MooBank.Modules.Assets.Tests.Support;

namespace Asm.MooBank.Modules.Assets.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingAsset_ReturnsAsset()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var asset = TestEntities.CreateAsset(
            id: assetId,
            name: "Test Asset",
            description: "Test description",
            value: 50000m,
            purchasePrice: 40000m);
        var queryable = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(assetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assetId, result.Id);
        Assert.Equal("Test Asset", result.Name);
        Assert.Equal("Test description", result.Description);
        Assert.Equal(50000m, result.CurrentBalance);
        Assert.Equal(40000m, result.PurchasePrice);
    }

    [Fact]
    public async Task Handle_MultipleAssets_ReturnsCorrectOne()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var assets = new[]
        {
            TestEntities.CreateAsset(name: "First Asset"),
            TestEntities.CreateAsset(id: targetId, name: "Target Asset", value: 25000m),
            TestEntities.CreateAsset(name: "Third Asset"),
        };
        var queryable = TestEntities.CreateAssetQueryable(assets);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target Asset", result.Name);
        Assert.Equal(25000m, result.CurrentBalance);
    }

    [Fact]
    public async Task Handle_NonExistentAsset_ThrowsNotFoundException()
    {
        // Arrange
        var assets = TestEntities.CreateSampleAssets();
        var queryable = TestEntities.CreateAssetQueryable(assets);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyCollection_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateAssetQueryable([]);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_AssetWithOwner_ReturnsGroupId()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var userId = _mocks.User.Id;
        var owner = TestEntities.CreateInstrumentOwner(userId: userId, groupId: groupId);
        var asset = TestEntities.CreateAsset(
            id: assetId,
            name: "Grouped Asset",
            owners: [owner]);
        var queryable = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(assetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(groupId, result.GroupId);
    }

    [Fact]
    public async Task Handle_CurrencyConversion_ReturnsConvertedBalance()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var asset = TestEntities.CreateAsset(
            id: assetId,
            value: 1000m,
            currency: "USD");
        var queryable = TestEntities.CreateAssetQueryable(asset);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(1000m, "USD"))
            .Returns(1500m);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CurrencyConverterMock.Object);
        var query = new Get(assetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1000m, result.CurrentBalance);
        Assert.Equal(1500m, result.CurrentBalanceLocalCurrency);
    }
}
