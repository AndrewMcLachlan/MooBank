using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="BudgetRepository"/> class.
/// Tests verify budget CRUD operations against an in-memory database.
/// </summary>
public class BudgetRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();

    public BudgetRepositoryTests()
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

    #region GetByYear

    /// <summary>
    /// Given a budget exists for a year
    /// When GetByYear is called
    /// Then the budget should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByYear_ExistingBudget_ReturnsBudget()
    {
        // Arrange
        var budget = new Budget(Guid.NewGuid())
        {
            FamilyId = _familyId,
            Year = 2024,
        };
        _context.Set<Budget>().Add(budget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new BudgetRepository(_context);

        // Act
        var result = await repository.GetByYear(_familyId, 2024, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Year);
        Assert.Equal(_familyId, result.FamilyId);
    }

    /// <summary>
    /// Given no budget exists for a year
    /// When GetByYear is called
    /// Then an exception should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByYear_NoBudget_ThrowsException()
    {
        // Arrange
        var repository = new BudgetRepository(_context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetByYear(_familyId, 2024, TestContext.Current.CancellationToken));
    }

    #endregion

    #region GetOrCreate

    /// <summary>
    /// Given a budget exists for a year
    /// When GetOrCreate is called
    /// Then the existing budget should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrCreate_ExistingBudget_ReturnsExisting()
    {
        // Arrange
        var existingBudget = new Budget(Guid.NewGuid())
        {
            FamilyId = _familyId,
            Year = 2024,
        };
        _context.Set<Budget>().Add(existingBudget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new BudgetRepository(_context);

        // Act
        var result = await repository.GetOrCreate(_familyId, 2024, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(existingBudget.Id, result.Id);
        Assert.Equal(2024, result.Year);
    }

    /// <summary>
    /// Given no budget exists for a year
    /// When GetOrCreate is called
    /// Then a new budget should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrCreate_NoBudget_CreatesNew()
    {
        // Arrange
        var repository = new BudgetRepository(_context);

        // Act
        var result = await repository.GetOrCreate(_familyId, 2025, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result.FamilyId);
        Assert.Equal(2025, result.Year);

        var savedBudget = await _context.Set<Budget>().FirstOrDefaultAsync(b => b.Year == 2025 && b.FamilyId == _familyId, TestContext.Current.CancellationToken);
        Assert.NotNull(savedBudget);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given a budget exists
    /// When Delete is called
    /// Then the budget should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingBudget_RemovesBudget()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = new Budget(budgetId)
        {
            FamilyId = _familyId,
            Year = 2024,
        };
        _context.Set<Budget>().Add(budget);
        await _context.SaveChangesAsync(true, TestContext.Current.CancellationToken);

        // Detach the entity so delete can work in the same context
        _context.Entry(budget).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var repository = new BudgetRepository(_context);

        // Act
        repository.Delete(budgetId);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedBudget = await _context.Set<Budget>().FindAsync([budgetId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deletedBudget);
    }

    #endregion

    #region DeleteLine

    /// <summary>
    /// Given a budget line exists
    /// When DeleteLine is called
    /// Then the line should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task DeleteLine_ExistingLine_RemovesLine()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var tag = new Tag(1) { Name = "Test Tag", FamilyId = _familyId };
        _context.Set<Tag>().Add(tag);

        var budget = new Budget(budgetId)
        {
            FamilyId = _familyId,
            Year = 2024,
        };
        var line = new BudgetLine(lineId)
        {
            BudgetId = budgetId,
            TagId = 1,
            Amount = 500m,
        };
        budget.Lines.Add(line);
        _context.Set<Budget>().Add(budget);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new BudgetRepository(_context);

        // Act
        repository.DeleteLine(lineId);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedLine = await _context.Set<BudgetLine>().FindAsync([lineId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(deletedLine);
    }

    #endregion
}
