#nullable enable

using Asm.Domain;
using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Eodhd;
using Asm.MooBank.Models;
using Asm.MooBank.Services;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="StockPriceService"/> service.
/// Tests cover stock price updates from external EODHD API.
/// </summary>
public class StockPriceServiceTests
{
    #region Update

    /// <summary>
    /// Given no stock holdings exist
    /// When Update is called
    /// Then no prices should be fetched
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_NoStockHoldings_FetchesNoPrices()
    {
        // Arrange
        var (service, stockPriceClientMock, _) = CreateService([], []);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert
        stockPriceClientMock.Verify(c => c.GetPriceAsync(It.IsAny<StockSymbol>()), Times.Never);
    }

    /// <summary>
    /// Given stock holdings exist
    /// When Update is called and prices are returned
    /// Then stock holdings should be updated with new prices
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_WithStockHoldings_UpdatesPrices()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");
        var holding = CreateStockHolding(symbol, 100m);
        var prices = new Dictionary<StockSymbol, decimal?> { { symbol, 150m } };

        var (service, _, _) = CreateService([holding], prices);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(150m, holding.CurrentPrice);
    }

    /// <summary>
    /// Given multiple stock holdings with different symbols
    /// When Update is called
    /// Then prices should be fetched for each unique symbol
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_MultipleSymbols_FetchesPricesForEach()
    {
        // Arrange
        var symbol1 = new StockSymbol("AAPL", "US");
        var symbol2 = new StockSymbol("GOOGL", "US");
        var holdings = new List<StockHolding>
        {
            CreateStockHolding(symbol1, 100m),
            CreateStockHolding(symbol2, 200m),
        };
        var prices = new Dictionary<StockSymbol, decimal?>
        {
            { symbol1, 150m },
            { symbol2, 2800m },
        };

        var (service, stockPriceClientMock, _) = CreateService(holdings, prices);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert
        stockPriceClientMock.Verify(c => c.GetPriceAsync(symbol1), Times.Once);
        stockPriceClientMock.Verify(c => c.GetPriceAsync(symbol2), Times.Once);
    }

    /// <summary>
    /// Given stock price API returns null for a symbol
    /// When Update is called
    /// Then that holding should not be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_PriceNotReturned_HoldingNotUpdated()
    {
        // Arrange
        var symbol = new StockSymbol("INVALID", "US");
        var holding = CreateStockHolding(symbol, 100m);
        var prices = new Dictionary<StockSymbol, decimal?> { { symbol, null } };

        var (service, _, _) = CreateService([holding], prices);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert - Price should remain unchanged
        Assert.Equal(100m, holding.CurrentPrice);
    }

    /// <summary>
    /// Given stock prices are updated
    /// When Update completes
    /// Then SaveChangesAsync should be called
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_Always_SavesChanges()
    {
        // Arrange
        var (service, _, unitOfWorkMock) = CreateService([], []);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Once);
    }

    /// <summary>
    /// Given a new price that doesn't exist in history
    /// When Update is called
    /// Then the price should be added to history
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_NewPrice_AddsToHistory()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");
        var holding = CreateStockHolding(symbol, 100m);
        var prices = new Dictionary<StockSymbol, decimal?> { { symbol, 150m } };

        var (service, _, _, repositoryMock) = CreateServiceWithRepository([holding], prices, []);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert
        repositoryMock.Verify(r => r.AddStockPrice(It.Is<StockPriceHistory>(
            p => p.Symbol == "AAPL" && p.Exchange == "US" && p.Price == 150m)), Times.Once);
    }

    /// <summary>
    /// Given a price that already exists in history
    /// When Update is called
    /// Then no duplicate history should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Update_ExistingPriceHistory_DoesNotAddDuplicate()
    {
        // Arrange
        var symbol = new StockSymbol("AAPL", "US");
        var holding = CreateStockHolding(symbol, 100m);
        var prices = new Dictionary<StockSymbol, decimal?> { { symbol, 150m } };
        var existingHistory = new List<StockPriceHistory>
        {
            new() { Symbol = "AAPL", Exchange = "US", Price = 145m, Date = DateOnlyExtensions.Today().AddDays(-1) }
        };

        var (service, _, _, repositoryMock) = CreateServiceWithRepository([holding], prices, existingHistory);

        // Act
        await service.Update(TestContext.Current.CancellationToken);

        // Assert - Should not add duplicate
        repositoryMock.Verify(r => r.AddStockPrice(It.IsAny<StockPriceHistory>()), Times.Never);
    }

    #endregion

    private static StockHolding CreateStockHolding(StockSymbol symbol, decimal currentPrice) =>
        new(Guid.NewGuid())
        {
            Name = "Test Stock",
            Symbol = symbol, // Implicit conversion from StockSymbol to StockSymbolEntity
            CurrentPrice = currentPrice,
            Quantity = 10,
            Currency = "USD",
        };

    private static (IStockPriceService service, Mock<IStockPriceClient> stockPriceClientMock, Mock<IUnitOfWork> unitOfWorkMock) CreateService(
        List<StockHolding> holdings,
        Dictionary<StockSymbol, decimal?> prices)
    {
        var (service, stockPriceClientMock, unitOfWorkMock, _) = CreateServiceWithRepository(holdings, prices, []);
        return (service, stockPriceClientMock, unitOfWorkMock);
    }

    private static (IStockPriceService service, Mock<IStockPriceClient> stockPriceClientMock, Mock<IUnitOfWork> unitOfWorkMock, Mock<IReferenceDataRepository> repositoryMock) CreateServiceWithRepository(
        List<StockHolding> holdings,
        Dictionary<StockSymbol, decimal?> prices,
        List<StockPriceHistory> existingHistory)
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var stockHoldingRepoMock = new Mock<IStockHoldingRepository>();
        var referenceDataRepoMock = new Mock<IReferenceDataRepository>();
        var stockPriceClientMock = new Mock<IStockPriceClient>();
        var loggerMock = new Mock<ILogger<StockPriceService>>();

        stockHoldingRepoMock.Setup(r => r.Get(It.IsAny<CancellationToken>()))
            .ReturnsAsync(holdings);

        referenceDataRepoMock.Setup(r => r.GetStockPrices(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHistory);

        foreach (var price in prices)
        {
            stockPriceClientMock.Setup(c => c.GetPriceAsync(price.Key))
                .ReturnsAsync(price.Value);
        }

        var service = new StockPriceService(
            unitOfWorkMock.Object,
            stockHoldingRepoMock.Object,
            referenceDataRepoMock.Object,
            stockPriceClientMock.Object,
            loggerMock.Object);

        return (service, stockPriceClientMock, unitOfWorkMock, referenceDataRepoMock);
    }
}
