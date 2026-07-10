#nullable enable
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="AuthorisationRepository"/> data queries used by
/// authorisation requirement handlers.
/// </summary>
[Trait("Category", "Unit")]
public class AuthorisationRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();

    public AuthorisationRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private AuthorisationRepository CreateRepository() => new(_context);

    #region IsGroupOwner

    /// <summary>
    /// Given a group owned by the user
    /// When IsGroupOwner is called
    /// Then true is returned
    /// </summary>
    [Fact]
    public async Task IsGroupOwner_UserOwnsGroup_ReturnsTrue()
    {
        // Arrange
        var group = TestEntities.CreateGroup(ownerId: _userId);
        _context.Groups.Add(group);
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().IsGroupOwner(group.Id, _userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Given a group owned by another user
    /// When IsGroupOwner is called
    /// Then false is returned
    /// </summary>
    [Fact]
    public async Task IsGroupOwner_UserDoesNotOwnGroup_ReturnsFalse()
    {
        // Arrange
        var group = TestEntities.CreateGroup(ownerId: Guid.NewGuid());
        _context.Groups.Add(group);
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().IsGroupOwner(group.Id, _userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Given a group that does not exist
    /// When IsGroupOwner is called
    /// Then false is returned
    /// </summary>
    [Fact]
    public async Task IsGroupOwner_GroupDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await CreateRepository().IsGroupOwner(Guid.NewGuid(), _userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetBudgetLineFamilyId

    /// <summary>
    /// Given a budget line
    /// When GetBudgetLineFamilyId is called
    /// Then the owning budget's family id is returned
    /// </summary>
    [Fact]
    public async Task GetBudgetLineFamilyId_LineExists_ReturnsFamilyId()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(familyId: _familyId);
        var line = TestEntities.CreateBudgetLine(budgetId: budget.Id);
        _context.Add(budget);
        _context.Add(line);
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().GetBudgetLineFamilyId(line.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result);
    }

    /// <summary>
    /// Given no budget line with the supplied id
    /// When GetBudgetLineFamilyId is called
    /// Then null is returned
    /// </summary>
    [Fact]
    public async Task GetBudgetLineFamilyId_LineDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await CreateRepository().GetBudgetLineFamilyId(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
