using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="ForecastRepository"/> class.
/// Tests verify forecast plan CRUD operations against an in-memory database.
/// </summary>
public class ForecastRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();

    public ForecastRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        SetupFamily();
    }

    private void SetupFamily()
    {
        var family = new Family(_familyId) { Name = "Test Family" };
        _context.Set<Family>().Add(family);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get

    /// <summary>
    /// Given forecast plans exist
    /// When Get is called
    /// Then all forecast plans should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingPlans_ReturnsAll()
    {
        // Arrange
        var plan1 = CreateForecastPlan(Guid.NewGuid(), "Plan 1");
        var plan2 = CreateForecastPlan(Guid.NewGuid(), "Plan 2");

        _context.Set<ForecastPlan>().AddRange(plan1, plan2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given a forecast plan exists
    /// When Get by id is called
    /// Then the plan should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingPlan_ReturnsPlan()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        var result = await repository.Get(planId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Plan", result.Name);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new forecast plan
    /// When Add is called
    /// Then the plan should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewPlan_PersistsPlan()
    {
        // Arrange
        var repository = new ForecastRepository(_context);
        var plan = CreateForecastPlan(Guid.NewGuid(), "New Plan");

        // Act
        repository.Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedPlan = await _context.Set<ForecastPlan>().FirstOrDefaultAsync(p => p.Name == "New Plan", TestContext.Current.CancellationToken);
        Assert.NotNull(savedPlan);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing forecast plan
    /// When Update is called
    /// Then the plan should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingPlan_UpdatesPlan()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Original Name");
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        plan.Name = "Updated Name";
        repository.Update(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedPlan = await _context.Set<ForecastPlan>().FindAsync([planId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedPlan!.Name);
    }

    #endregion

    #region Domain Methods

    /// <summary>
    /// Given a forecast plan
    /// When Archive is called
    /// Then the plan should be archived
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Archive_ExistingPlan_SetsIsArchived()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        plan.Archive();
        repository.Update(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var archivedPlan = await _context.Set<ForecastPlan>().FindAsync([planId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(archivedPlan!.IsArchived);
    }

    /// <summary>
    /// Given an archived forecast plan
    /// When Restore is called
    /// Then the plan should be restored
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Restore_ArchivedPlan_ClearsIsArchived()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        plan.IsArchived = true;
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        plan.Restore();
        repository.Update(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var restoredPlan = await _context.Set<ForecastPlan>().FindAsync([planId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(restoredPlan!.IsArchived);
    }

    #endregion

    #region AddPlannedItem

    /// <summary>
    /// Given a forecast plan
    /// When AddPlannedItem is called
    /// Then the item should be added to the collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddPlannedItem_NewItem_AddsToCollection()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        var item = new ForecastPlannedItem(Guid.NewGuid())
        {
            Name = "Salary",
            Amount = 5000m,
            ItemType = PlannedItemType.Income,
            DateMode = PlannedItemDateMode.FixedDate,
        };

        // Act
        var result = plan.AddPlannedItem(item);

        // Assert
        Assert.Single(plan.PlannedItems);
        Assert.Equal("Salary", plan.PlannedItems.First().Name);
        Assert.Equal(planId, plan.PlannedItems.First().ForecastPlanId);
        Assert.Same(item, result);
    }

    /// <summary>
    /// Given a forecast plan with existing items
    /// When AddPlannedItem is called multiple times
    /// Then all items should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddPlannedItem_MultipleItems_AddsAllToCollection()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");

        var item1 = new ForecastPlannedItem(Guid.NewGuid())
        {
            Name = "Salary",
            Amount = 5000m,
            ItemType = PlannedItemType.Income,
            DateMode = PlannedItemDateMode.FixedDate,
        };
        var item2 = new ForecastPlannedItem(Guid.NewGuid())
        {
            Name = "Rent",
            Amount = 2000m,
            ItemType = PlannedItemType.Expense,
            DateMode = PlannedItemDateMode.FixedDate,
        };

        // Act
        plan.AddPlannedItem(item1);
        plan.AddPlannedItem(item2);

        // Assert
        Assert.Equal(2, plan.PlannedItems.Count);
        Assert.Contains(plan.PlannedItems, i => i.Name == "Salary");
        Assert.Contains(plan.PlannedItems, i => i.Name == "Rent");
    }

    /// <summary>
    /// Given a forecast plan
    /// When AddPlannedItem is called
    /// Then UpdatedUtc should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddPlannedItem_UpdatesUpdatedUtc()
    {
        // Arrange
        var plan = CreateForecastPlan(Guid.NewGuid(), "Test Plan");
        var originalUpdated = plan.UpdatedUtc;
        var item = new ForecastPlannedItem(Guid.NewGuid())
        {
            Name = "Test",
            Amount = 100m,
            ItemType = PlannedItemType.Income,
            DateMode = PlannedItemDateMode.FixedDate,
        };

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        plan.AddPlannedItem(item);

        // Assert
        Assert.True(plan.UpdatedUtc > originalUpdated);
    }

    #endregion

    #region RemovePlannedItem

    /// <summary>
    /// Given a forecast plan with items
    /// When RemovePlannedItem is called
    /// Then the item should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RemovePlannedItem_ExistingItem_RemovesFromCollection()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        var item = new ForecastPlannedItem(itemId)
        {
            Name = "Salary",
            Amount = 5000m,
            ForecastPlanId = planId,
            ItemType = PlannedItemType.Income,
            DateMode = PlannedItemDateMode.FixedDate,
        };
        plan.PlannedItems.Add(item);
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new ForecastRepository(_context);

        // Act
        plan.RemovePlannedItem(itemId);
        repository.Update(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedPlan = await _context.Set<ForecastPlan>()
            .Include(p => p.PlannedItems)
            .FirstAsync(p => p.Id == planId, TestContext.Current.CancellationToken);
        Assert.Empty(savedPlan.PlannedItems);
    }

    /// <summary>
    /// Given a forecast plan
    /// When RemovePlannedItem is called with non-existent id
    /// Then nothing should happen
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RemovePlannedItem_NonExistentItem_DoesNothing()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        var item = new ForecastPlannedItem(Guid.NewGuid())
        {
            Name = "Salary",
            Amount = 5000m,
            ForecastPlanId = planId,
            ItemType = PlannedItemType.Income,
            DateMode = PlannedItemDateMode.FixedDate,
        };
        plan.PlannedItems.Add(item);
        _context.Set<ForecastPlan>().Add(plan);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        plan.RemovePlannedItem(Guid.NewGuid()); // Non-existent ID

        // Assert - item should still exist
        Assert.Single(plan.PlannedItems);
    }

    #endregion

    #region SetAccounts

    /// <summary>
    /// Given a forecast plan
    /// When SetAccounts is called
    /// Then the in-memory collection should have the accounts
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccounts_WithInstrumentIds_SetsAccountsInMemory()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");

        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();

        // Act
        plan.SetAccounts([instrumentId1, instrumentId2]);

        // Assert
        Assert.Equal(2, plan.Accounts.Count);
        Assert.Contains(plan.Accounts, a => a.InstrumentId == instrumentId1);
        Assert.Contains(plan.Accounts, a => a.InstrumentId == instrumentId2);
        Assert.All(plan.Accounts, a => Assert.Equal(planId, a.ForecastPlanId));
    }

    /// <summary>
    /// Given a forecast plan with existing accounts
    /// When SetAccounts is called with new ids
    /// Then the in-memory collection should be replaced
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccounts_ReplacesExistingAccountsInMemory()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var oldInstrumentId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        plan.Accounts.Add(new ForecastPlanAccount { ForecastPlanId = planId, InstrumentId = oldInstrumentId });

        var newInstrumentId = Guid.NewGuid();

        // Act
        plan.SetAccounts([newInstrumentId]);

        // Assert
        Assert.Single(plan.Accounts);
        Assert.Equal(newInstrumentId, plan.Accounts.First().InstrumentId);
        Assert.DoesNotContain(plan.Accounts, a => a.InstrumentId == oldInstrumentId);
    }

    /// <summary>
    /// Given a forecast plan
    /// When SetAccounts is called with empty list
    /// Then the accounts collection should be cleared
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccounts_WithEmptyList_ClearsAccounts()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = CreateForecastPlan(planId, "Test Plan");
        plan.Accounts.Add(new ForecastPlanAccount { ForecastPlanId = planId, InstrumentId = Guid.NewGuid() });

        // Act
        plan.SetAccounts([]);

        // Assert
        Assert.Empty(plan.Accounts);
    }

    #endregion

    private ForecastPlan CreateForecastPlan(Guid id, string name) =>
        new(id)
        {
            Name = name,
            FamilyId = _familyId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            AccountScopeMode = AccountScopeMode.AllAccounts,
            StartingBalanceMode = StartingBalanceMode.CalculatedCurrent,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };
}
