#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.Instruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.Instruments;

[Trait("Category", "Unit")]
public class GetListTests
{
    private readonly TestMocks _mocks;

    public GetListTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithLogicalAccounts_ReturnsAccountsInList()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account1 = TestEntities.CreateInstrumentWithOwner(name: "Checking", ownerId: userId);
        var account2 = TestEntities.CreateInstrumentWithOwner(name: "Savings", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account1, account2);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Name == "Checking");
        Assert.Contains(result, r => r.Name == "Savings");
    }

    [Fact]
    public async Task Handle_WithStockHoldings_ReturnsStockHoldingsInList()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var holding = TestEntities.CreateStockHolding(name: "Apple Shares", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable(holding);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Apple Shares", result.First().Name);
    }

    [Fact]
    public async Task Handle_WithAssets_ReturnsAssetsInList()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var asset = TestEntities.CreateAsset(name: "House", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("House", result.First().Name);
    }

    [Fact]
    public async Task Handle_WithMixedInstruments_ReturnsAllTypes()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = TestEntities.CreateInstrumentWithOwner(name: "Checking", ownerId: userId);
        var holding = TestEntities.CreateStockHolding(name: "Stocks", ownerId: userId);
        var asset = TestEntities.CreateAsset(name: "Car", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable(holding);
        var assets = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_NoInstruments_ReturnsEmptyList()
    {
        // Arrange
        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsSortedByName()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var accountZ = TestEntities.CreateInstrumentWithOwner(name: "Zebra", ownerId: userId);
        var accountA = TestEntities.CreateInstrumentWithOwner(name: "Alpha", ownerId: userId);
        var accountM = TestEntities.CreateInstrumentWithOwner(name: "Mango", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(accountZ, accountA, accountM);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var names = result.Select(r => r.Name).ToList();
        Assert.Equal("Alpha", names[0]);
        Assert.Equal("Mango", names[1]);
        Assert.Equal("Zebra", names[2]);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectIdAndName()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var accountId = Guid.NewGuid();
        var account = TestEntities.CreateInstrumentWithOwner(id: accountId, name: "Test Account", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetListHandler(logicalAccounts, stockHoldings, assets, _mocks.User);
        var query = new GetList();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var item = result.First();
        Assert.Equal(accountId, item.Id);
        Assert.Equal("Test Account", item.Name);
    }
}
