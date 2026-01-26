using Asm.MooBank.Domain.Entities.Family;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="FamilyRepository"/> class.
/// Tests verify family CRUD operations against an in-memory database.
/// </summary>
public class FamilyRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public FamilyRepositoryTests()
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
    /// Given families exist
    /// When Get is called
    /// Then all families should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingFamilies_ReturnsAll()
    {
        // Arrange
        var family1 = new Family(Guid.NewGuid()) { Name = "Family 1" };
        var family2 = new Family(Guid.NewGuid()) { Name = "Family 2" };

        _context.Set<Family>().AddRange(family1, family2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new FamilyRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given a family exists
    /// When Get by id is called
    /// Then the family should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingFamily_ReturnsFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family(familyId) { Name = "Test Family" };
        _context.Set<Family>().Add(family);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new FamilyRepository(_context);

        // Act
        var result = await repository.Get(familyId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Family", result.Name);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new family
    /// When Add is called
    /// Then the family should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewFamily_PersistsFamily()
    {
        // Arrange
        var repository = new FamilyRepository(_context);
        var family = new Family(Guid.NewGuid()) { Name = "New Family" };

        // Act
        repository.Add(family);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedFamily = await _context.Set<Family>().FirstOrDefaultAsync(f => f.Name == "New Family", TestContext.Current.CancellationToken);
        Assert.NotNull(savedFamily);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing family
    /// When Update is called
    /// Then the family should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingFamily_UpdatesFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new Family(familyId) { Name = "Original Name" };
        _context.Set<Family>().Add(family);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new FamilyRepository(_context);

        // Act
        family.Name = "Updated Name";
        repository.Update(family);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedFamily = await _context.Set<Family>().FindAsync([familyId], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedFamily!.Name);
    }

    #endregion
}
