using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="InstitutionRepository"/> class.
/// Tests verify institution CRUD operations against an in-memory database.
/// </summary>
public class InstitutionRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;

    public InstitutionRepositoryTests()
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
    /// Given institutions exist
    /// When Get is called
    /// Then all institutions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingInstitutions_ReturnsAll()
    {
        // Arrange
        var institution1 = CreateInstitution(1, "Bank A", InstitutionType.Bank);
        var institution2 = CreateInstitution(2, "Credit Union B", InstitutionType.CreditUnion);

        _context.Set<Institution>().AddRange(institution1, institution2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstitutionRepository(_context);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given an institution exists
    /// When Get by id is called
    /// Then the institution should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingInstitution_ReturnsInstitution()
    {
        // Arrange
        var institution = CreateInstitution(1, "Test Bank", InstitutionType.Bank);
        _context.Set<Institution>().Add(institution);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstitutionRepository(_context);

        // Act
        var result = await repository.Get(1, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Bank", result.Name);
        Assert.Equal(InstitutionType.Bank, result.InstitutionType);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new institution
    /// When Add is called
    /// Then the institution should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewInstitution_PersistsInstitution()
    {
        // Arrange
        var repository = new InstitutionRepository(_context);
        var institution = CreateInstitution(1, "New Bank", InstitutionType.Bank);

        // Act
        repository.Add(institution);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedInstitution = await _context.Set<Institution>().FirstOrDefaultAsync(i => i.Name == "New Bank", TestContext.Current.CancellationToken);
        Assert.NotNull(savedInstitution);
    }

    #endregion

    #region Update

    /// <summary>
    /// Given an existing institution
    /// When Update is called
    /// Then the institution should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Update_ExistingInstitution_UpdatesInstitution()
    {
        // Arrange
        var institution = CreateInstitution(1, "Original Name", InstitutionType.Bank);
        _context.Set<Institution>().Add(institution);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new InstitutionRepository(_context);

        // Act
        institution.Name = "Updated Name";
        repository.Update(institution);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var updatedInstitution = await _context.Set<Institution>().FindAsync([1], cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated Name", updatedInstitution!.Name);
    }

    #endregion

    private static Institution CreateInstitution(int id, string name, InstitutionType type) =>
        new(id)
        {
            Name = name,
            InstitutionType = type,
        };
}
