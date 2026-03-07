#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Queries;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Tests.Queries;

[Trait("Category", "Unit")]
public class GetPlanTests
{
    private readonly TestMocks _mocks;

    public GetPlanTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingPlan_ReturnsPlan()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(
            id: planId,
            name: "Test Plan",
            familyId: familyId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 12, 31));

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(planId, result.Id);
        Assert.Equal("Test Plan", result.Name);
    }

    [Fact]
    public async Task Handle_ExistingPlan_ReturnsCorrectDates()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var startDate = new DateOnly(2024, 3, 1);
        var endDate = new DateOnly(2025, 2, 28);
        var plan = TestEntities.CreateForecastPlan(
            id: planId,
            familyId: familyId,
            startDate: startDate,
            endDate: endDate);

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
    }

    [Fact]
    public async Task Handle_ExistingPlan_ReturnsBalanceSettings()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(
            id: planId,
            familyId: familyId,
            startingBalanceMode: StartingBalanceMode.ManualAmount,
            startingBalanceAmount: 5000m);

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(StartingBalanceMode.ManualAmount, result.StartingBalanceMode);
        Assert.Equal(5000m, result.StartingBalanceAmount);
    }

    [Fact]
    public async Task Handle_ExistingPlan_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertFamilyPermission(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentPlan_ThrowsException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingPlan = TestEntities.CreateForecastPlan(familyId: familyId);
        var nonExistentPlanId = Guid.NewGuid();

        var plans = TestEntities.CreatePlanQueryable(existingPlan);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(nonExistentPlanId);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
        Assert.True(
            exception is InvalidOperationException ||
            (exception.InnerException is InvalidOperationException),
            "Expected InvalidOperationException or wrapped InvalidOperationException");
    }

    [Fact]
    public async Task Handle_PlanWithPlannedItems_ReturnsPlannedItems()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plannedItems = new[]
        {
            TestEntities.CreatePlannedItem(planId: planId, name: "Item 1", amount: 100m),
            TestEntities.CreatePlannedItem(planId: planId, name: "Item 2", amount: 200m),
        };
        var plan = TestEntities.CreateForecastPlan(
            id: planId,
            familyId: familyId,
            plannedItems: plannedItems);

        var plans = TestEntities.CreatePlanQueryable(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new GetPlanHandler(plans, _mocks.SecurityMock.Object);
        var query = new GetPlan(planId);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.PlannedItems.Count());
    }
}
