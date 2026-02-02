using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="StockHoldingRepository"/> class.
/// Tests verify stock holding CRUD operations against an in-memory database.
/// </summary>
public class StockHoldingRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();

    public StockHoldingRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given stock holdings exist
    /// When Get is called
    /// Then all stock holdings should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingHoldings_ReturnsAllHoldings()
    {
        // Arrange
        var holding1 = CreateStockHolding(Guid.NewGuid(), "AAPL Holdings");
        var holding2 = CreateStockHolding(Guid.NewGuid(), "MSFT Holdings");

        _context.Set<StockHolding>().Add(holding1);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.Set<StockHolding>().Add(holding2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given no stock holdings exist
    /// When Get is called
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_NoHoldings_ReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Get By Id

    /// <summary>
    /// Given a stock holding exists
    /// When Get by id is called
    /// Then the stock holding should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingHolding_ReturnsHolding()
    {
        // Arrange
        var holdingId = Guid.NewGuid();
        var holding = CreateStockHolding(holdingId, "Test Holding");
        _context.Set<StockHolding>().Add(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(holdingId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Holding", result.Name);
    }

    /// <summary>
    /// Given a stock holding does not exist
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_NonExistentHolding_ThrowsNotFoundException()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            repository.Get(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new stock holding
    /// When Add is called
    /// Then the holding should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewHolding_PersistsHolding()
    {
        // Arrange
        var repository = CreateRepository();
        var holding = CreateStockHolding(Guid.NewGuid(), "New Holding");

        // Act
        repository.Add(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedHolding = await _context.Set<StockHolding>()
            .FirstOrDefaultAsync(h => h.Name == "New Holding", TestContext.Current.CancellationToken);
        Assert.NotNull(savedHolding);
    }

    /// <summary>
    /// Given a stock holding with current price
    /// When Add is called
    /// Then current price should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_WithCurrentPrice_PersistsPrice()
    {
        // Arrange
        var repository = CreateRepository();
        var holding = CreateStockHolding(Guid.NewGuid(), "Price Test");
        holding.CurrentPrice = 175.50m;

        // Act
        repository.Add(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedHolding = await _context.Set<StockHolding>()
            .FirstOrDefaultAsync(h => h.Name == "Price Test", TestContext.Current.CancellationToken);
        Assert.Equal(175.50m, savedHolding!.CurrentPrice);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing stock holding
    /// When Update is called
    /// Then the holding should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingHolding_UpdatesHolding()
    {
        // Arrange
        var holdingId = Guid.NewGuid();
        var holding = CreateStockHolding(holdingId, "Original Name");
        _context.Set<StockHolding>().Add(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        holding.Name = "Updated Name";
        repository.Update(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedHolding = await _context.Set<StockHolding>()
            .FindAsync([holdingId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedHolding!.Name);
    }

    /// <summary>
    /// Given a stock holding
    /// When CurrentPrice is updated
    /// Then the price should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_CurrentPrice_PersistsPrice()
    {
        // Arrange
        var holdingId = Guid.NewGuid();
        var holding = CreateStockHolding(holdingId, "Price Test");
        holding.CurrentPrice = 100.00m;
        _context.Set<StockHolding>().Add(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        holding.CurrentPrice = 150.50m;
        repository.Update(holding);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedHolding = await _context.Set<StockHolding>()
            .FindAsync([holdingId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(150.50m, updatedHolding!.CurrentPrice);
    }

    #endregion

    #region Helpers

    private StockHoldingRepository CreateRepository() =>
        new(_context);

    private StockHolding CreateStockHolding(Guid id, string name)
    {
        var userId = Guid.NewGuid();
        var user = new Domain.Entities.User.User(userId)
        {
            EmailAddress = $"user-{userId}@test.com",
            FamilyId = _familyId,
        };
        _context.Set<Domain.Entities.User.User>().Add(user);

        return new StockHolding(id)
        {
            Name = name,
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = userId, User = user }],
        };
    }

    #endregion
}
