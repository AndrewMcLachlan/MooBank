#nullable enable
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="AuthorisationRepository"/> data queries used by
/// authorisation requirement handlers.
/// </summary>
[Trait("Category", "Integration")]
public class AuthorisationRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
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
        var groupId = Guid.NewGuid();
        _context.Set<Group>().Add(new Group(groupId) { Name = "Test Group", OwnerId = _userId });
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().IsGroupOwner(groupId, _userId, TestContext.Current.CancellationToken);

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
        var groupId = Guid.NewGuid();
        _context.Set<Group>().Add(new Group(groupId) { Name = "Other Group", OwnerId = Guid.NewGuid() });
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().IsGroupOwner(groupId, _userId, TestContext.Current.CancellationToken);

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
        var lineId = AddBudgetLine(_familyId);

        // Act
        var result = await CreateRepository().GetBudgetLineFamilyId(lineId, TestContext.Current.CancellationToken);

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

    #region GetOwnedInstrumentIds

    /// <summary>
    /// Given instruments owned by the user and by others
    /// When GetOwnedInstrumentIds is called
    /// Then only the user's instrument ids are returned
    /// </summary>
    [Fact]
    public async Task GetOwnedInstrumentIds_ReturnsOnlyUsersInstruments()
    {
        // Arrange
        var ownedId = Guid.NewGuid();
        _context.Set<InstrumentOwner>().Add(new InstrumentOwner { InstrumentId = ownedId, UserId = _userId });
        _context.Set<InstrumentOwner>().Add(new InstrumentOwner { InstrumentId = Guid.NewGuid(), UserId = Guid.NewGuid() });
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().GetOwnedInstrumentIds(_userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([ownedId], result);
    }

    #endregion

    private Guid AddBudgetLine(Guid familyId)
    {
        var budget = new Budget(Guid.NewGuid())
        {
            FamilyId = familyId,
            Year = 2024,
        };
        var line = new BudgetLine(Guid.NewGuid())
        {
            BudgetId = budget.Id,
            TagId = 1,
            Amount = 100m,
        };

        _context.Set<Budget>().Add(budget);
        _context.Set<BudgetLine>().Add(line);
        _context.SaveChanges();

        return line.Id;
    }
}
