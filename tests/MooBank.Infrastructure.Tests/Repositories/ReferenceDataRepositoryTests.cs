#nullable enable
using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

[Trait("Category", "Unit")]
public class ReferenceDataRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;

    public ReferenceDataRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region AddStockPrice

    [Fact]
    public void AddStockPrice_NewStockPrice_AddsToContext()
    {
        // Arrange
        var stockPrice = TestEntities.CreateStockPriceHistory(
            symbol: "AAPL",
            date: DateOnly.FromDateTime(DateTime.Today),
            price: 150m);

        var repository = CreateRepository();

        // Act
        var result = repository.AddStockPrice(stockPrice);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(stockPrice.Symbol, result.Symbol);

        var entry = _context.ChangeTracker.Entries<StockPriceHistory>()
            .SingleOrDefault(e => e.Entity.Symbol == stockPrice.Symbol && e.Entity.Date == stockPrice.Date);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    [Fact]
    public void AddStockPrice_ExistingStockPrice_DoesNotAddDuplicate()
    {
        // Arrange
        var symbol = "AAPL";
        var date = DateOnly.FromDateTime(DateTime.Today);

        var existingStockPrice = TestEntities.CreateStockPriceHistory(symbol: symbol, date: date, price: 150m);
        _context.StockPriceHistory.Add(existingStockPrice);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var newStockPrice = TestEntities.CreateStockPriceHistory(symbol: symbol, date: date, price: 155m);

        var repository = CreateRepository();

        // Act
        var result = repository.AddStockPrice(newStockPrice);

        // Assert - should return the input but not add it to the context
        Assert.NotNull(result);

        var addedEntries = _context.ChangeTracker.Entries<StockPriceHistory>()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added);

        Assert.Empty(addedEntries);
    }

    [Fact]
    public void AddStockPrice_SameSymbolDifferentDate_AddsToContext()
    {
        // Arrange
        var symbol = "AAPL";
        var existingDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var newDate = DateOnly.FromDateTime(DateTime.Today);

        var existingStockPrice = TestEntities.CreateStockPriceHistory(symbol: symbol, date: existingDate, price: 150m);
        _context.StockPriceHistory.Add(existingStockPrice);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var newStockPrice = TestEntities.CreateStockPriceHistory(symbol: symbol, date: newDate, price: 155m);

        var repository = CreateRepository();

        // Act
        var result = repository.AddStockPrice(newStockPrice);

        // Assert - different date should be added
        var entry = _context.ChangeTracker.Entries<StockPriceHistory>()
            .SingleOrDefault(e => e.Entity.Date == newDate);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    [Fact]
    public void AddStockPrice_DifferentSymbolSameDate_AddsToContext()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);

        var existingStockPrice = TestEntities.CreateStockPriceHistory(symbol: "AAPL", date: date, price: 150m);
        _context.StockPriceHistory.Add(existingStockPrice);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var newStockPrice = TestEntities.CreateStockPriceHistory(symbol: "GOOGL", date: date, price: 2800m);

        var repository = CreateRepository();

        // Act
        var result = repository.AddStockPrice(newStockPrice);

        // Assert - different symbol should be added
        var entry = _context.ChangeTracker.Entries<StockPriceHistory>()
            .SingleOrDefault(e => e.Entity.Symbol == "GOOGL");

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    #endregion

    #region AddExchangeRate

    [Fact]
    public void AddExchangeRate_AddsToContext()
    {
        // Arrange
        var exchangeRate = new ExchangeRate
        {
            From = "AUD",
            To = "USD",
            Rate = 0.65m,
        };

        var repository = CreateRepository();

        // Act
        var result = repository.AddExchangeRate(exchangeRate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AUD", result.From);

        var entry = _context.ChangeTracker.Entries<ExchangeRate>()
            .SingleOrDefault(e => e.Entity.From == "AUD" && e.Entity.To == "USD");

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    #endregion

    #region GetExchangeRates

    [Fact]
    public async Task GetExchangeRates_ReturnsAllExchangeRates()
    {
        // Arrange
        var rate1 = new ExchangeRate { From = "AUD", To = "USD", Rate = 0.65m };
        var rate2 = new ExchangeRate { From = "AUD", To = "EUR", Rate = 0.60m };

        _context.ExchangeRates.AddRange(rate1, rate2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetExchangeRates(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetExchangeRates_NoRates_ReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetExchangeRates(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetStockPrices by Date

    [Fact]
    public async Task GetStockPrices_ByDate_ReturnsMatchingPrices()
    {
        // Arrange
        var targetDate = DateOnly.FromDateTime(DateTime.Today);
        var otherDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var price1 = TestEntities.CreateStockPriceHistory(symbol: "AAPL", date: targetDate, price: 150m);
        var price2 = TestEntities.CreateStockPriceHistory(symbol: "GOOGL", date: targetDate, price: 2800m);
        var price3 = TestEntities.CreateStockPriceHistory(symbol: "MSFT", date: otherDate, price: 400m);

        _context.StockPriceHistory.AddRange(price1, price2, price3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetStockPrices(targetDate, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(targetDate, p.Date));
    }

    #endregion

    #region GetStockPrices by Symbol

    [Fact]
    public async Task GetStockPrices_BySymbol_ReturnsOnlyMatchingSymbolAndExchange()
    {
        // Arrange
        var date1 = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));
        var date2 = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var aaplYesterday = TestEntities.CreateStockPriceHistory(symbol: "AAPL", exchange: null, date: date2, price: 150m);
        var aaplDayBefore = TestEntities.CreateStockPriceHistory(symbol: "AAPL", exchange: null, date: date1, price: 148m);
        var googl = TestEntities.CreateStockPriceHistory(symbol: "GOOGL", exchange: null, date: date2, price: 2800m);
        var aaplOnDifferentExchange = TestEntities.CreateStockPriceHistory(symbol: "AAPL", exchange: "AU", date: date2, price: 200m);

        _context.StockPriceHistory.AddRange(aaplYesterday, aaplDayBefore, googl, aaplOnDifferentExchange);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = (await repository.GetStockPrices(new StockSymbol("AAPL", null), TestContext.Current.CancellationToken)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("AAPL", p.Symbol));
        Assert.All(result, p => Assert.Null(p.Exchange));
    }

    [Fact]
    public async Task GetStockPrices_BySymbol_MatchesNullExchangeOnly()
    {
        // Arrange - a symbol with a non-null exchange should not match when querying by null exchange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var withoutExchange = TestEntities.CreateStockPriceHistory(symbol: "VAS", exchange: null, date: date, price: 100m);
        var withExchange = TestEntities.CreateStockPriceHistory(symbol: "VAS", exchange: "AU", date: date, price: 105m);

        _context.StockPriceHistory.AddRange(withoutExchange, withExchange);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = (await repository.GetStockPrices(new StockSymbol("VAS", "AU"), TestContext.Current.CancellationToken)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(105m, result[0].Price);
        Assert.Equal("AU", result[0].Exchange);
    }

    [Fact]
    public async Task GetStockPrices_BySymbol_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var existing = TestEntities.CreateStockPriceHistory(symbol: "AAPL", exchange: null, date: date, price: 150m);
        _context.StockPriceHistory.Add(existing);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetStockPrices(new StockSymbol("NOTHERE", null), TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetImporterTypes

    [Fact]
    public async Task GetImporterTypes_ReturnsAllImporterTypes()
    {
        // Arrange
        var importerType1 = TestEntities.CreateImporterType(id: 1, name: "CSV Importer");
        var importerType2 = TestEntities.CreateImporterType(id: 2, name: "OFX Importer");

        _context.ImporterTypes.AddRange(importerType1, importerType2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetImporterTypes(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region AddCpiChange

    [Fact]
    public void AddCpiChange_AddsToContext()
    {
        // Arrange
        var cpiChange = new CpiChange
        {
            Quarter = new QuarterEntity(2024, 1),
            ChangePercent = 1.5m,
        };

        var repository = CreateRepository();

        // Act
        var result = repository.AddCpiChange(cpiChange);

        // Assert
        Assert.NotNull(result);

        var entry = _context.ChangeTracker.Entries<CpiChange>()
            .SingleOrDefault(e => e.Entity.Quarter.Year == 2024 && e.Entity.Quarter.QuarterNumber == 1);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    #endregion

    #region GetCpiChanges

    [Fact]
    public async Task GetCpiChanges_ReturnsAllCpiChanges()
    {
        // Arrange
        var cpi1 = new CpiChange { Quarter = new QuarterEntity(2024, 1), ChangePercent = 1.5m };
        var cpi2 = new CpiChange { Quarter = new QuarterEntity(2024, 2), ChangePercent = 1.2m };

        _context.CpiChanges.AddRange(cpi1, cpi2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetCpiChanges(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    private ReferenceDataRepository CreateRepository() => new(_context);
}
