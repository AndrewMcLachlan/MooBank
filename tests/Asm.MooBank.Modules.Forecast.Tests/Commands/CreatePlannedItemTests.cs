#nullable enable
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class CreatePlannedItemTests
{
    private readonly TestMocks _mocks;

    public CreatePlannedItemTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsItemToPlan()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(name: "New Item", amount: 250m);
        var command = new CreatePlannedItem(planId, item);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(plan.PlannedItems);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedItem()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(name: "New Item", amount: 250m, itemType: PlannedItemType.Income);
        var command = new CreatePlannedItem(planId, item);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Item", result.Name);
        Assert.Equal(250m, result.Amount);
        Assert.Equal(PlannedItemType.Income, result.ItemType);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(name: "New Item");
        var command = new CreatePlannedItem(planId, item);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(name: "New Item");
        var command = new CreatePlannedItem(planId, item);

        // Act
        await handler.Handle(command, CancellationToken.None);

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

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(name: "New Item");
        var command = new CreatePlannedItem(planId, item);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WithFixedDate_SetsFixedDateOnEntity()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var targetDate = new DateOnly(2025, 6, 15);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new CreatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var item = TestEntities.CreatePlannedItemModel(
            name: "Fixed Date Item",
            dateMode: PlannedItemDateMode.FixedDate,
            fixedDate: targetDate);
        var command = new CreatePlannedItem(planId, item);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(PlannedItemDateMode.FixedDate, result.DateMode);
        Assert.Equal(targetDate, result.FixedDate);
    }
}
