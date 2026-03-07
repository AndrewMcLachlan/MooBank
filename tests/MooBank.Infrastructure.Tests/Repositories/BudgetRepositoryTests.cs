#nullable enable
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

[Trait("Category", "Unit")]
public class BudgetRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;

    public BudgetRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetOrCreate

    [Fact]
    public async Task GetOrCreate_BudgetExists_ReturnExistingBudget()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short year = 2024;
        var existingBudget = TestEntities.CreateBudget(familyId: familyId, year: year);

        _context.Add(existingBudget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetOrCreate(familyId, year, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingBudget.Id, result.Id);
        Assert.Equal(familyId, result.FamilyId);
        Assert.Equal(year, result.Year);
    }

    [Fact]
    public async Task GetOrCreate_BudgetDoesNotExist_CreatesNewBudget()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short year = 2024;

        var repository = CreateRepository();

        // Act
        var result = await repository.GetOrCreate(familyId, year, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(familyId, result.FamilyId);
        Assert.Equal(year, result.Year);
    }

    [Fact]
    public async Task GetOrCreate_BudgetDoesNotExist_AddsToChangeTracker()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short year = 2024;

        var repository = CreateRepository();

        // Act
        var result = await repository.GetOrCreate(familyId, year, TestContext.Current.CancellationToken);

        // Assert - budget should be tracked for insertion
        var trackedEntry = _context.ChangeTracker.Entries<Budget>()
            .SingleOrDefault(e => e.Entity.FamilyId == familyId && e.Entity.Year == year);

        Assert.NotNull(trackedEntry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, trackedEntry.State);
    }

    [Fact]
    public async Task GetOrCreate_DifferentYearExists_CreatesNewBudgetForRequestedYear()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short existingYear = 2023;
        short requestedYear = 2024;
        var existingBudget = TestEntities.CreateBudget(familyId: familyId, year: existingYear);

        _context.Add(existingBudget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetOrCreate(familyId, requestedYear, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(existingBudget.Id, result.Id);
        Assert.Equal(requestedYear, result.Year);
    }

    [Fact]
    public async Task GetOrCreate_DifferentFamilyExists_CreatesNewBudgetForRequestedFamily()
    {
        // Arrange
        var existingFamilyId = Guid.NewGuid();
        var requestedFamilyId = Guid.NewGuid();
        short year = 2024;
        var existingBudget = TestEntities.CreateBudget(familyId: existingFamilyId, year: year);

        _context.Add(existingBudget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetOrCreate(requestedFamilyId, year, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(existingBudget.Id, result.Id);
        Assert.Equal(requestedFamilyId, result.FamilyId);
    }

    #endregion

    #region DeleteLine

    [Fact]
    public void DeleteLine_EntityInChangeTracker_RemovesTrackedEntity()
    {
        // Arrange
        var budgetLine = TestEntities.CreateBudgetLine();
        _context.Add(budgetLine);
        _context.SaveChanges();

        // Detach and reattach to simulate it being tracked
        _context.ChangeTracker.Clear();
        _context.Attach(budgetLine);

        var repository = CreateRepository();

        // Act
        repository.DeleteLine(budgetLine.Id);

        // Assert
        var entry = _context.ChangeTracker.Entries<BudgetLine>()
            .SingleOrDefault(e => e.Entity.Id == budgetLine.Id);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Deleted, entry.State);
    }

    [Fact]
    public void DeleteLine_EntityNotInChangeTracker_CreatesStubAndDeletes()
    {
        // Arrange - entity is not in change tracker
        var budgetLineId = Guid.NewGuid();

        var repository = CreateRepository();

        // Act
        repository.DeleteLine(budgetLineId);

        // Assert - a stub entity should be created and marked for deletion
        var entry = _context.ChangeTracker.Entries<BudgetLine>()
            .SingleOrDefault(e => e.Entity.Id == budgetLineId);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Deleted, entry.State);
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_MarksBudgetForDeletion()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var repository = CreateRepository();

        // Act
        repository.Delete(budgetId);

        // Assert
        var entry = _context.ChangeTracker.Entries<Budget>()
            .SingleOrDefault(e => e.Entity.Id == budgetId);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Deleted, entry.State);
    }

    #endregion

    #region AddLine

    [Fact]
    public void AddLine_AddsLineToContext()
    {
        // Arrange
        var tag = TestEntities.CreateTag(id: 1, familyId: Guid.NewGuid());
        _context.Add(tag);
        _context.SaveChanges();

        var budgetLine = TestEntities.CreateBudgetLine(tagId: tag.Id);

        var repository = CreateRepository();

        // Act
        var result = repository.AddLine(budgetLine);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(budgetLine.Id, result.Id);

        var entry = _context.ChangeTracker.Entries<BudgetLine>()
            .SingleOrDefault(e => e.Entity.Id == budgetLine.Id);

        Assert.NotNull(entry);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Added, entry.State);
    }

    #endregion

    #region GetByYear

    [Fact]
    public async Task GetByYear_BudgetExists_ReturnsBudget()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short year = 2024;
        var budget = TestEntities.CreateBudget(familyId: familyId, year: year);

        _context.Add(budget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.GetByYear(familyId, year, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(budget.Id, result.Id);
    }

    [Fact]
    public async Task GetByYear_BudgetDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        short year = 2024;

        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetByYear(familyId, year, TestContext.Current.CancellationToken));
    }

    #endregion

    private BudgetRepository CreateRepository() => new(_context);
}
