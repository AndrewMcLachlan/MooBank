#nullable enable
using Asm.MooBank.Modules.Forecast.Queries;
using Asm.MooBank.Modules.Forecast.Tests.Support;

namespace Asm.MooBank.Modules.Forecast.Tests.Queries;

[Trait("Category", "Unit")]
public class GetPlansTests
{
    private readonly TestMocks _mocks;

    public GetPlansTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithPlans_ReturnsUserFamilyPlans()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var plans = new[]
        {
            TestEntities.CreateForecastPlan(name: "Plan 1", familyId: familyId),
            TestEntities.CreateForecastPlan(name: "Plan 2", familyId: familyId),
        };
        var queryable = TestEntities.CreatePlanQueryable(plans);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_NoPlans_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreatePlanQueryable([]);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FiltersToUserFamily()
    {
        // Arrange
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();
        var plans = new[]
        {
            TestEntities.CreateForecastPlan(name: "User Plan 1", familyId: userFamilyId),
            TestEntities.CreateForecastPlan(name: "User Plan 2", familyId: userFamilyId),
            TestEntities.CreateForecastPlan(name: "Other Family Plan", familyId: otherFamilyId),
        };
        var queryable = TestEntities.CreatePlanQueryable(plans);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.StartsWith("User Plan", p.Name));
    }

    [Fact]
    public async Task Handle_ExcludesArchivedByDefault()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var plans = new[]
        {
            TestEntities.CreateForecastPlan(name: "Active Plan", familyId: familyId, isArchived: false),
            TestEntities.CreateForecastPlan(name: "Archived Plan", familyId: familyId, isArchived: true),
        };
        var queryable = TestEntities.CreatePlanQueryable(plans);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans(IncludeArchived: false);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Plan", result.First().Name);
    }

    [Fact]
    public async Task Handle_IncludesArchivedWhenRequested()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var plans = new[]
        {
            TestEntities.CreateForecastPlan(name: "Active Plan", familyId: familyId, isArchived: false),
            TestEntities.CreateForecastPlan(name: "Archived Plan", familyId: familyId, isArchived: true),
        };
        var queryable = TestEntities.CreatePlanQueryable(plans);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans(IncludeArchived: true);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_OrdersByUpdatedUtcDescending()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var oldPlan = TestEntities.CreateForecastPlan(name: "Old Plan", familyId: familyId);
        oldPlan.UpdatedUtc = DateTime.UtcNow.AddDays(-10);

        var newPlan = TestEntities.CreateForecastPlan(name: "New Plan", familyId: familyId);
        newPlan.UpdatedUtc = DateTime.UtcNow;

        var middlePlan = TestEntities.CreateForecastPlan(name: "Middle Plan", familyId: familyId);
        middlePlan.UpdatedUtc = DateTime.UtcNow.AddDays(-5);

        var plans = new[] { oldPlan, newPlan, middlePlan };
        var queryable = TestEntities.CreatePlanQueryable(plans);

        var handler = new GetPlansHandler(queryable, _mocks.User);
        var query = new GetPlans();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Equal("New Plan", resultList[0].Name);
        Assert.Equal("Middle Plan", resultList[1].Name);
        Assert.Equal("Old Plan", resultList[2].Name);
    }
}
