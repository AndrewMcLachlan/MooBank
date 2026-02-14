#nullable enable
using Asm.MooBank.Modules.Instruments.Queries.Instruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Queries.Instruments;

[Trait("Category", "Unit")]
public class GetFormattedTests
{
    private readonly TestMocks _mocks;

    public GetFormattedTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_NoInstruments_ReturnsEmptyGroups()
    {
        // Arrange
        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Groups);
    }

    [Fact]
    public async Task Handle_InstrumentsWithNoGroup_ReturnsOtherAccountsGroup()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = TestEntities.CreateInstrumentWithOwner(name: "Checking", ownerId: userId, balance: 1000m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Groups);
        var group = result.Groups.First();
        Assert.Equal("Other Accounts", group.Name);
        Assert.Null(group.Id);
    }

    [Fact]
    public async Task Handle_InstrumentsWithNoGroup_GroupContainsInstruments()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account1 = TestEntities.CreateInstrumentWithOwner(name: "Checking", ownerId: userId);
        var account2 = TestEntities.CreateInstrumentWithOwner(name: "Savings", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account1, account2);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var group = result.Groups.First();
        Assert.Equal(2, group.Instruments.Count());
    }

    [Fact]
    public async Task Handle_MixedInstrumentTypes_AllIncludedInOtherAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = TestEntities.CreateInstrumentWithOwner(name: "Checking", ownerId: userId);
        var holding = TestEntities.CreateStockHolding(name: "Stocks", ownerId: userId);
        var asset = TestEntities.CreateAsset(name: "Car", ownerId: userId);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable(holding);
        var assets = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var group = result.Groups.First();
        Assert.Equal(3, group.Instruments.Count());
    }

    [Fact]
    public async Task Handle_CalculatesGroupTotal()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account1 = TestEntities.CreateInstrumentWithOwner(name: "A", ownerId: userId, balance: 1000m);
        var account2 = TestEntities.CreateInstrumentWithOwner(name: "B", ownerId: userId, balance: 500m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account1, account2);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var group = result.Groups.First();
        Assert.Equal(1500m, group.Total);
    }

    [Fact]
    public async Task Handle_UsesCurrencyConverter()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var account = TestEntities.CreateInstrumentWithOwner(name: "USD Account", ownerId: userId, balance: 100m, currency: "USD");

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var instrument = result.Groups.First().Instruments.First();
        Assert.Equal(100m, instrument.CurrentBalance);
        Assert.Equal(150m, instrument.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public async Task Handle_InstrumentWithVirtualAccounts_IncludesVirtualAccounts()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var vi1 = TestEntities.CreateVirtualInstrument(name: "Savings Goal");
        var vi2 = TestEntities.CreateVirtualInstrument(name: "Emergency Fund");
        var account = TestEntities.CreateInstrument(
            name: "Main Account",
            virtualInstruments: [vi1, vi2]);
        account.Owners.Add(new Domain.Entities.Instrument.InstrumentOwner { UserId = userId, InstrumentId = account.Id });

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var instrument = result.Groups.First().Instruments.First();
        Assert.NotNull(instrument.VirtualInstruments);
        Assert.Equal(2, instrument.VirtualInstruments.Count());
    }

    [Fact]
    public async Task Handle_StockHolding_ReturnsAsSharesType()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var holding = TestEntities.CreateStockHolding(name: "AAPL", ownerId: userId, currentValue: 5000m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable(holding);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var instrument = result.Groups.First().Instruments.First();
        Assert.Equal("Shares", instrument.InstrumentType);
        Assert.Equal(5000m, instrument.CurrentBalance);
    }

    [Fact]
    public async Task Handle_Asset_ReturnsAsAssetType()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var asset = TestEntities.CreateAsset(name: "House", ownerId: userId, value: 500000m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable([]);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var instrument = result.Groups.First().Instruments.First();
        Assert.Equal("Asset", instrument.InstrumentType);
        Assert.Equal(500000m, instrument.CurrentBalance);
    }

    [Fact]
    public async Task Handle_InstrumentsWithGroup_GroupsInstrumentsCorrectly()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();

        var group = new Domain.Entities.Group.Group(groupId)
        {
            Name = "Savings Accounts",
            OwnerId = userId,
            ShowPosition = true,
        };

        var account1 = TestEntities.CreateInstrumentWithOwner(name: "Account 1", ownerId: userId, balance: 1000m);
        var account2 = TestEntities.CreateInstrumentWithOwner(name: "Account 2", ownerId: userId, balance: 2000m);

        // Assign accounts to the group through their owners
        account1.Owners.First().Group = group;
        account1.Owners.First().GroupId = groupId;
        account2.Owners.First().Group = group;
        account2.Owners.First().GroupId = groupId;

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account1, account2);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Groups);
        var resultGroup = result.Groups.First();
        Assert.Equal("Savings Accounts", resultGroup.Name);
        Assert.Equal(groupId, resultGroup.Id);
        Assert.True(resultGroup.ShowTotal);
        Assert.Equal(2, resultGroup.Instruments.Count());
        Assert.Equal(3000m, resultGroup.Total);
    }

    [Fact]
    public async Task Handle_MixedGroupedAndUngroupedInstruments_ReturnsMultipleGroups()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();

        var group = new Domain.Entities.Group.Group(groupId)
        {
            Name = "Main Accounts",
            OwnerId = userId,
            ShowPosition = false,
        };

        // Grouped account
        var groupedAccount = TestEntities.CreateInstrumentWithOwner(name: "Grouped", ownerId: userId, balance: 1000m);
        groupedAccount.Owners.First().Group = group;
        groupedAccount.Owners.First().GroupId = groupId;

        // Ungrouped account
        var ungroupedAccount = TestEntities.CreateInstrumentWithOwner(name: "Ungrouped", ownerId: userId, balance: 500m);

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(groupedAccount, ungroupedAccount);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable([]);
        var assets = TestEntities.CreateAssetQueryable([]);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Groups.Count());

        var mainGroup = result.Groups.FirstOrDefault(g => g.Name == "Main Accounts");
        Assert.NotNull(mainGroup);
        Assert.Single(mainGroup.Instruments);
        Assert.False(mainGroup.ShowTotal);

        var otherGroup = result.Groups.FirstOrDefault(g => g.Name == "Other Accounts");
        Assert.NotNull(otherGroup);
        Assert.Single(otherGroup.Instruments);
    }

    [Fact]
    public async Task Handle_MultipleInstrumentTypesInGroup_CombinesAllTypes()
    {
        // Arrange
        var userId = _mocks.User.Id;
        var groupId = Guid.NewGuid();

        var group = new Domain.Entities.Group.Group(groupId)
        {
            Name = "Investments",
            OwnerId = userId,
            ShowPosition = true,
        };

        // Add account with group
        var account = TestEntities.CreateInstrumentWithOwner(name: "Investment Account", ownerId: userId, balance: 1000m);
        account.Owners.First().Group = group;
        account.Owners.First().GroupId = groupId;

        // Add stock holding with group
        var stockHolding = TestEntities.CreateStockHolding(name: "Stocks", ownerId: userId, currentValue: 5000m);
        stockHolding.Owners.First().Group = group;
        stockHolding.Owners.First().GroupId = groupId;

        // Add asset with group
        var asset = TestEntities.CreateAsset(name: "Property", ownerId: userId, value: 50000m);
        asset.Owners.First().Group = group;
        asset.Owners.First().GroupId = groupId;

        var logicalAccounts = TestEntities.CreateLogicalAccountQueryable(account);
        var stockHoldings = TestEntities.CreateStockHoldingQueryable(stockHolding);
        var assets = TestEntities.CreateAssetQueryable(asset);

        var handler = new GetFormattedHandler(
            logicalAccounts,
            stockHoldings,
            assets,
            _mocks.User,
            _mocks.CurrencyConverterMock.Object);

        var query = new GetFormatted();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Groups);
        var resultGroup = result.Groups.First();
        Assert.Equal("Investments", resultGroup.Name);
        Assert.Equal(3, resultGroup.Instruments.Count());
        Assert.Equal(56000m, resultGroup.Total); // 1000 + 5000 + 50000
    }
}
