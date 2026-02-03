#nullable enable
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Models;

[Trait("Category", "Unit")]
public class InstrumentExtensionsTests
{
    private readonly TestMocks _mocks;

    public InstrumentExtensionsTests()
    {
        _mocks = new TestMocks();
    }

    #region LogicalAccount.ToModel

    [Fact]
    public void LogicalAccount_ToModel_MapsBasicProperties()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new LogicalAccount(accountId, [])
        {
            Name = "Test Account",
            Description = "Test Description",
            Currency = "AUD",
            Balance = 1000m,
            AccountType = AccountType.Transaction,
            Controller = Controller.Manual,
        };

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(accountId, model.Id);
        Assert.Equal("Test Account", model.Name);
        Assert.Equal("Test Description", model.Description);
        Assert.Equal("AUD", model.Currency);
        Assert.Equal(1000m, model.CurrentBalance);
        Assert.Equal(Controller.Manual, model.Controller);
    }

    [Fact]
    public void LogicalAccount_ToModel_MapsAccountType()
    {
        // Arrange
        var account = new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Savings",
            Currency = "AUD",
            AccountType = AccountType.Savings,
        };

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal("Savings", model.InstrumentType);
    }

    [Fact]
    public void LogicalAccount_ToModel_ConvertsCurrency()
    {
        // Arrange
        var account = new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "USD Account",
            Currency = "USD",
            Balance = 100m,
        };

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m);

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(100m, model.CurrentBalance);
        Assert.Equal(150m, model.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public void LogicalAccount_ToModel_WithNoVirtualInstruments_ReturnsEmptyArray()
    {
        // Arrange
        var account = new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test",
            Currency = "AUD",
        };

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Empty(model.VirtualInstruments);
    }

    [Fact]
    public void LogicalAccount_ToModel_WithVirtualInstruments_MapsVirtualInstruments()
    {
        // Arrange
        var account = TestEntities.CreateInstrument(
            name: "Main Account",
            virtualInstruments: [
                TestEntities.CreateVirtualInstrument(name: "Savings Goal"),
                TestEntities.CreateVirtualInstrument(name: "Emergency")
            ]);

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(2, model.VirtualInstruments.Count());
    }

    [Fact]
    public void LogicalAccount_ToModel_ExcludesClosedVirtualInstruments()
    {
        // Arrange
        var openVi = TestEntities.CreateVirtualInstrument(name: "Open");
        var closedVi = TestEntities.CreateVirtualInstrument(name: "Closed");
        closedVi.ClosedDate = DateOnly.FromDateTime(DateTime.Today);

        var account = TestEntities.CreateInstrument(
            name: "Main Account",
            virtualInstruments: [openVi, closedVi]);

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Single(model.VirtualInstruments);
        Assert.Equal("Open", model.VirtualInstruments.First().Name);
    }

    [Fact]
    public void LogicalAccount_ToModel_CalculatesRemainingBalance()
    {
        // Arrange
        var vi1 = TestEntities.CreateVirtualInstrument(name: "VI1", balance: 300m);
        var vi2 = TestEntities.CreateVirtualInstrument(name: "VI2", balance: 200m);
        var account = TestEntities.CreateInstrument(
            name: "Main",
            virtualInstruments: [vi1, vi2]);
        account.Balance = 1000m;

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(500m, model.RemainingBalance); // 1000 - 300 - 200
    }

    [Fact]
    public void LogicalAccount_ToModel_NoVirtualInstruments_RemainingBalanceIsNull()
    {
        // Arrange
        var account = new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test",
            Currency = "AUD",
            Balance = 1000m,
        };

        // Act
        var model = account.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Null(model.RemainingBalance);
    }

    [Fact]
    public void LogicalAccountCollection_ToModel_MapsAllAccounts()
    {
        // Arrange
        var account1 = new LogicalAccount(Guid.NewGuid(), []) { Name = "A", Currency = "AUD" };
        var account2 = new LogicalAccount(Guid.NewGuid(), []) { Name = "B", Currency = "AUD" };
        var accounts = new[] { account1, account2 };

        // Act
        var models = accounts.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(2, models.Count());
    }

    #endregion

    #region StockHolding.ToModel

    [Fact]
    public void StockHolding_ToModel_MapsBasicProperties()
    {
        // Arrange
        var holdingId = Guid.NewGuid();
        var holding = TestEntities.CreateStockHolding(
            id: holdingId,
            name: "Apple Shares",
            description: "AAPL holdings",
            currency: "USD",
            currentValue: 5000m);

        // Act
        var model = holding.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(holdingId, model.Id);
        Assert.Equal("Apple Shares", model.Name);
        Assert.Equal("AAPL holdings", model.Description);
        Assert.Equal("USD", model.Currency);
        Assert.Equal(5000m, model.CurrentBalance);
    }

    [Fact]
    public void StockHolding_ToModel_SetsInstrumentTypeToShares()
    {
        // Arrange
        var holding = TestEntities.CreateStockHolding(name: "Stocks");

        // Act
        var model = holding.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal("Shares", model.InstrumentType);
    }

    [Fact]
    public void StockHolding_ToModel_ConvertsCurrency()
    {
        // Arrange
        var holding = TestEntities.CreateStockHolding(currency: "USD", currentValue: 100m);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m);

        // Act
        var model = holding.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(100m, model.CurrentBalance);
        Assert.Equal(150m, model.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public void StockHoldingCollection_ToModel_MapsAllHoldings()
    {
        // Arrange
        var holdings = new[]
        {
            TestEntities.CreateStockHolding(name: "A"),
            TestEntities.CreateStockHolding(name: "B")
        };

        // Act
        var models = holdings.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(2, models.Count());
    }

    #endregion

    #region Asset.ToModel

    [Fact]
    public void Asset_ToModel_MapsBasicProperties()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var asset = TestEntities.CreateAsset(
            id: assetId,
            name: "House",
            description: "Primary residence",
            currency: "AUD",
            value: 500000m);

        // Act
        var model = asset.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(assetId, model.Id);
        Assert.Equal("House", model.Name);
        Assert.Equal("Primary residence", model.Description);
        Assert.Equal("AUD", model.Currency);
        Assert.Equal(500000m, model.CurrentBalance);
    }

    [Fact]
    public void Asset_ToModel_SetsInstrumentTypeToAsset()
    {
        // Arrange
        var asset = TestEntities.CreateAsset(name: "Car");

        // Act
        var model = asset.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal("Asset", model.InstrumentType);
    }

    [Fact]
    public void Asset_ToModel_ConvertsCurrency()
    {
        // Arrange
        var asset = TestEntities.CreateAsset(currency: "EUR", value: 100m);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "EUR"))
            .Returns(165m);

        // Act
        var model = asset.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(100m, model.CurrentBalance);
        Assert.Equal(165m, model.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public void AssetCollection_ToModel_MapsAllAssets()
    {
        // Arrange
        var assets = new[]
        {
            TestEntities.CreateAsset(name: "A"),
            TestEntities.CreateAsset(name: "B")
        };

        // Act
        var models = assets.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(2, models.Count());
    }

    #endregion

    #region VirtualInstrument.ToModel

    [Fact]
    public void VirtualInstrument_ToModel_MapsBasicProperties()
    {
        // Arrange
        var viId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var vi = TestEntities.CreateVirtualInstrument(
            id: viId,
            parentId: parentId,
            name: "Savings Goal",
            description: "For vacation",
            currency: "AUD",
            balance: 2500m,
            controller: Controller.Virtual);

        // Act
        var model = vi.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(viId, model.Id);
        Assert.Equal(parentId, model.ParentId);
        Assert.Equal("Savings Goal", model.Name);
        Assert.Equal("For vacation", model.Description);
        Assert.Equal("AUD", model.Currency);
        Assert.Equal(2500m, model.CurrentBalance);
        Assert.Equal(Controller.Virtual, model.Controller);
    }

    [Fact]
    public void VirtualInstrument_ToModel_ConvertsCurrency()
    {
        // Arrange
        var vi = TestEntities.CreateVirtualInstrument(currency: "USD", balance: 100m);

        _mocks.CurrencyConverterMock
            .Setup(c => c.Convert(100m, "USD"))
            .Returns(150m);

        // Act
        var model = vi.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(100m, model.CurrentBalance);
        Assert.Equal(150m, model.CurrentBalanceLocalCurrency);
    }

    [Fact]
    public void VirtualInstrumentCollection_ToModel_MapsAllInstruments()
    {
        // Arrange
        var instruments = new[]
        {
            TestEntities.CreateVirtualInstrument(name: "A"),
            TestEntities.CreateVirtualInstrument(name: "B")
        };

        // Act
        var models = instruments.ToModel(_mocks.CurrencyConverterMock.Object);

        // Assert
        Assert.Equal(2, models.Count());
    }

    #endregion

    #region InstrumentSummary.VirtualAccountRemainingBalance

    [Fact]
    public void InstrumentSummary_VirtualAccountRemainingBalance_CalculatesCorrectly()
    {
        // Arrange
        var summary = new InstrumentSummary
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CurrentBalance = 1000m,
            CurrentBalanceLocalCurrency = 1000m,
            Controller = Controller.Manual,
            Currency = "AUD",
            VirtualInstruments = new[]
            {
                new MooBank.Models.VirtualInstrument { Id = Guid.NewGuid(), Name = "A", CurrentBalance = 300m, CurrentBalanceLocalCurrency = 300m, Controller = Controller.Virtual,Currency = "AUD", },
                new MooBank.Models.VirtualInstrument { Id = Guid.NewGuid(), Name = "B", CurrentBalance = 200m, CurrentBalanceLocalCurrency = 300m, Controller = Controller.Virtual,Currency = "AUD", }
            }
        };

        // Act
        var remaining = summary.VirtualAccountRemainingBalance;

        // Assert
        Assert.Equal(500m, remaining); // 1000 - 300 - 200
    }

    [Fact]
    public void InstrumentSummary_VirtualAccountRemainingBalance_NoVirtualInstruments_ReturnsFullBalance()
    {
        // Arrange
        var summary = new InstrumentSummary
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CurrentBalance = 1000m,
            CurrentBalanceLocalCurrency = 1000m,
            Controller = Controller.Manual,
            Currency = "AUD",
            VirtualInstruments = null!
        };

        // Act
        var remaining = summary.VirtualAccountRemainingBalance;

        // Assert
        Assert.Equal(1000m, remaining);
    }

    [Fact]
    public void InstrumentSummary_VirtualAccountRemainingBalance_EmptyVirtualInstruments_ReturnsFullBalance()
    {
        // Arrange
        var summary = new InstrumentSummary
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CurrentBalance = 1000m,
            CurrentBalanceLocalCurrency = 1000m,
            Controller = Controller.Manual,
            Currency = "AUD",
            VirtualInstruments = []
        };

        // Act
        var remaining = summary.VirtualAccountRemainingBalance;

        // Assert
        Assert.Equal(1000m, remaining);
    }

    #endregion
}
