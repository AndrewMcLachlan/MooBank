#nullable enable
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;
using ForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using Tag = Asm.MooBank.Domain.Entities.Tag.Tag;
using TagSettings = Asm.MooBank.Domain.Entities.Tag.TagSettings;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="AuthorisationReader"/> data queries used by
/// authorisation requirement handlers.
/// </summary>
[Trait("Category", "Integration")]
public class AuthorisationReaderTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();

    public AuthorisationReaderTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private AuthorisationReader CreateRepository() => new(_context);

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
        var tagId = AddTag(_familyId);

        // Act
        var result = await CreateRepository().GetTagFamilyId(tagId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result);
    }

    /// <summary>
    /// Given a soft-deleted tag
    /// When GetTagFamilyId is called
    /// Then the tag's family id is still returned (the query ignores the query filters)
    /// </summary>
    [Fact]
    public async Task GetTagFamilyId_TagDeleted_ReturnsFamilyId()
    {
        // Arrange
        var tagId = AddTag(_familyId, deleted: true);

        // Act
        var result = await CreateRepository().GetTagFamilyId(tagId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(_familyId, result);
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
        var planId = AddForecastPlan(_familyId);

        // Act
        var result = await CreateRepository().GetForecastPlanFamilyId(planId, TestContext.Current.CancellationToken);

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

    private int AddTag(Guid familyId, bool deleted = false)
    {
        var tag = new Tag(1)
        {
            Name = "Test Tag",
            FamilyId = familyId,
            Deleted = deleted,
            Settings = new TagSettings(1),
        };

        _context.Set<Tag>().Add(tag);
        _context.SaveChanges();

        return tag.Id;
    }

    private Guid AddForecastPlan(Guid familyId)
    {
        var plan = new ForecastPlan(Guid.NewGuid())
        {
            FamilyId = familyId,
            Name = "Test Plan",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        _context.Set<ForecastPlan>().Add(plan);
        _context.SaveChanges();

        return plan.Id;
    }
}
