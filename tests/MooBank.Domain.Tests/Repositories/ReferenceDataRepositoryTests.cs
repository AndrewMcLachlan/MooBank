using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="ReferenceDataRepository"/> class.
/// Tests verify reference data operations against an in-memory database.
/// </summary>
public class ReferenceDataRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public ReferenceDataRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetExchangeRates

    /// <summary>
    /// Given exchange rates exist
    /// When GetExchangeRates is called
    /// Then all exchange rates should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetExchangeRates_WithExistingRates_ReturnsAll()
    {
        // Arrange
        var rate1 = new ExchangeRate { From = "USD", To = "AUD", Rate = 1.50m };
        var rate2 = new ExchangeRate { From = "GBP", To = "AUD", Rate = 1.85m };

        _context.ExchangeRates.AddRange(rate1, rate2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ReferenceDataRepository(_context);

        // Act
        var result = await repository.GetExchangeRates(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region AddExchangeRate

    /// <summary>
    /// Given a new exchange rate
    /// When AddExchangeRate is called
    /// Then the rate should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddExchangeRate_NewRate_PersistsRate()
    {
        // Arrange
        var repository = new ReferenceDataRepository(_context);
        var rate = new ExchangeRate { From = "EUR", To = "AUD", Rate = 1.65m };

        // Act
        repository.AddExchangeRate(rate);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedRate = await _context.ExchangeRates.FirstOrDefaultAsync(r => r.From == "EUR", TestContext.Current.CancellationToken);
        Assert.NotNull(savedRate);
        Assert.Equal(1.65m, savedRate.Rate);
    }

    #endregion

    #region GetCpiChanges

    /// <summary>
    /// Given CPI changes exist
    /// When GetCpiChanges is called
    /// Then all CPI changes should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCpiChanges_WithExistingChanges_ReturnsAll()
    {
        // Arrange
        var change1 = new CpiChange { Quarter = new Asm.MooBank.Domain.Entities.QuarterEntity(2024, 1), ChangePercent = 1.2m };
        var change2 = new CpiChange { Quarter = new Asm.MooBank.Domain.Entities.QuarterEntity(2024, 2), ChangePercent = 0.8m };

        _context.CpiChanges.AddRange(change1, change2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ReferenceDataRepository(_context);

        // Act
        var result = await repository.GetCpiChanges(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region AddCpiChange

    /// <summary>
    /// Given a new CPI change
    /// When AddCpiChange is called
    /// Then the change should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddCpiChange_NewChange_PersistsChange()
    {
        // Arrange
        var repository = new ReferenceDataRepository(_context);
        var change = new CpiChange { Quarter = new Asm.MooBank.Domain.Entities.QuarterEntity(2024, 3), ChangePercent = 1.5m };

        // Act
        repository.AddCpiChange(change);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedChange = await _context.CpiChanges.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedChange);
        Assert.Equal(1.5m, savedChange.ChangePercent);
    }

    #endregion

    #region AddStockPrice

    /// <summary>
    /// Given a new stock price
    /// When AddStockPrice is called
    /// Then the price should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddStockPrice_NewPrice_PersistsPrice()
    {
        // Arrange
        var repository = new ReferenceDataRepository(_context);
        var price = new StockPriceHistory
        {
            Symbol = "AAPL",
            Exchange = "US",
            Date = DateOnly.FromDateTime(DateTime.Today),
            Price = 150.25m
        };

        // Act
        repository.AddStockPrice(price);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedPrice = await _context.StockPriceHistory.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedPrice);
        Assert.Equal(150.25m, savedPrice.Price);
    }

    /// <summary>
    /// Given an existing stock price for the same symbol and date
    /// When AddStockPrice is called with duplicate
    /// Then no duplicate should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddStockPrice_DuplicatePrice_DoesNotAddDuplicate()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Today);
        var existingPrice = new StockPriceHistory { Symbol = "GOOGL", Exchange = "US", Date = date, Price = 2800.00m };

        _context.StockPriceHistory.Add(existingPrice);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ReferenceDataRepository(_context);
        var duplicatePrice = new StockPriceHistory { Symbol = "GOOGL", Exchange = "US", Date = date, Price = 2850.00m };

        // Act
        repository.AddStockPrice(duplicatePrice);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var count = await _context.StockPriceHistory.CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, count);

        // Original price should be unchanged
        var savedPrice = await _context.StockPriceHistory.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2800.00m, savedPrice!.Price);
    }

    #endregion

    #region GetStockPrices by Date

    /// <summary>
    /// Given stock prices exist for a date
    /// When GetStockPrices is called with that date
    /// Then prices for that date should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetStockPrices_ByDate_ReturnsMatchingPrices()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var yesterday = today.AddDays(-1);

        var price1 = new StockPriceHistory { Symbol = "AAPL", Exchange = "US", Date = today, Price = 150.00m };
        var price2 = new StockPriceHistory { Symbol = "MSFT", Exchange = "US", Date = today, Price = 380.00m };
        var price3 = new StockPriceHistory { Symbol = "AAPL", Exchange = "US", Date = yesterday, Price = 148.00m };

        _context.StockPriceHistory.AddRange(price1, price2, price3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ReferenceDataRepository(_context);

        // Act
        var result = await repository.GetStockPrices(today, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(today, p.Date));
    }

    #endregion
}
