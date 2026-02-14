#nullable enable
using Asm.MooBank.Modules.Forecast.Queries;
using Asm.MooBank.Modules.Forecast.Tests.Support;

namespace Asm.MooBank.Modules.Forecast.Tests.Queries;

[Trait("Category", "Unit")]
public class GetPlannedItemTests
{
    private readonly TestMocks _mocks;

    public GetPlannedItemTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingItem_ReturnsItem()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var plannedItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, name: "Test Item");
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [plannedItem]);
        var queryable = TestEntities.CreatePlanQueryable(plan);

        var handler = new GetPlannedItemHandler(queryable, _mocks.User);
        var query = new GetPlannedItem(planId, itemId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
        Assert.Equal("Test Item", result.Name);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var queryable = TestEntities.CreatePlanQueryable(plan);

        var handler = new GetPlannedItemHandler(queryable, _mocks.User);
        var query = new GetPlannedItem(planId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreatePlanQueryable([]);

        var handler = new GetPlannedItemHandler(queryable, _mocks.User);
        var query = new GetPlannedItem(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_PlanFromDifferentFamily_ThrowsNotFoundException()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var plannedItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, name: "Test Item");
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: otherFamilyId, plannedItems: [plannedItem]);
        var queryable = TestEntities.CreatePlanQueryable(plan);

        var handler = new GetPlannedItemHandler(queryable, _mocks.User);
        var query = new GetPlannedItem(planId, itemId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_MultipleItems_ReturnsCorrectItem()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var targetItemId = Guid.NewGuid();
        var items = new[]
        {
            TestEntities.CreatePlannedItem(planId: planId, name: "Item 1"),
            TestEntities.CreatePlannedItem(id: targetItemId, planId: planId, name: "Target Item"),
            TestEntities.CreatePlannedItem(planId: planId, name: "Item 3"),
        };
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: items);
        var queryable = TestEntities.CreatePlanQueryable(plan);

        var handler = new GetPlannedItemHandler(queryable, _mocks.User);
        var query = new GetPlannedItem(planId, targetItemId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(targetItemId, result.Id);
        Assert.Equal("Target Item", result.Name);
    }
}
