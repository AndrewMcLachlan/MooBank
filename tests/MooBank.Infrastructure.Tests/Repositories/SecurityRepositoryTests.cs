#nullable enable
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

/// <summary>
/// Unit tests for the <see cref="SecurityRepository"/> data queries used by
/// authorisation requirement handlers.
/// </summary>
[Trait("Category", "Unit")]
public class SecurityRepositoryTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();

    public SecurityRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private SecurityRepository CreateRepository() => new(_context);

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

    #region GetTagFamilyId

    /// <summary>
    /// Given a tag
    /// When GetTagFamilyId is called
    /// Then the tag's family id is returned
    /// </summary>
    [Fact]
    public async Task GetTagFamilyId_TagExists_ReturnsFamilyId()
    {
        // Arrange
        var tag = TestEntities.CreateTag(id: 1, familyId: _familyId);
        tag.Settings = TestEntities.CreateTagSettings(tag.Id);
        _context.Add(tag);
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().GetTagFamilyId(tag.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result);
    }

    /// <summary>
    /// Given a soft-deleted tag belonging to another family
    /// When GetTagFamilyId is called
    /// Then the tag's family id is still returned (the query ignores the query filters)
    /// </summary>
    [Fact]
    public async Task GetTagFamilyId_DeletedTagInOtherFamily_ReturnsFamilyId()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        using var userScopedContext = TestDbContextFactory.Create(TestEntities.CreateUserModel(familyId: _familyId));
        var tag = TestEntities.CreateTag(id: 1, familyId: otherFamilyId, deleted: true);
        tag.Settings = TestEntities.CreateTagSettings(tag.Id);
        userScopedContext.Add(tag);
        userScopedContext.SaveChanges();

        // Act
        var result = await new SecurityRepository(userScopedContext).GetTagFamilyId(tag.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(otherFamilyId, result);
    }

    /// <summary>
    /// Given no tag with the supplied id
    /// When GetTagFamilyId is called
    /// Then null is returned
    /// </summary>
    [Fact]
    public async Task GetTagFamilyId_TagDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await CreateRepository().GetTagFamilyId(999, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetForecastPlanFamilyId

    /// <summary>
    /// Given a forecast plan
    /// When GetForecastPlanFamilyId is called
    /// Then the plan's family id is returned
    /// </summary>
    [Fact]
    public async Task GetForecastPlanFamilyId_PlanExists_ReturnsFamilyId()
    {
        // Arrange
        var plan = new Domain.Entities.Forecast.ForecastPlan(Guid.NewGuid())
        {
            FamilyId = _familyId,
            Name = "Test Plan",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };
        _context.Add(plan);
        _context.SaveChanges();

        // Act
        var result = await CreateRepository().GetForecastPlanFamilyId(plan.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result);
    }

    /// <summary>
    /// Given no forecast plan with the supplied id
    /// When GetForecastPlanFamilyId is called
    /// Then null is returned
    /// </summary>
    [Fact]
    public async Task GetForecastPlanFamilyId_PlanDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await CreateRepository().GetForecastPlanFamilyId(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
