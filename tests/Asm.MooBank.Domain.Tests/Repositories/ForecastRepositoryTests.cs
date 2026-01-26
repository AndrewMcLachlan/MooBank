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
