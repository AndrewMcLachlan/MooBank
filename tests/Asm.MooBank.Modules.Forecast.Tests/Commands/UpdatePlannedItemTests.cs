#nullable enable
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using ModelPlannedItem = Asm.MooBank.Modules.Forecast.Models.PlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdatePlannedItemTests
{
    private readonly TestMocks _mocks;

    public UpdatePlannedItemTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesItemName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, name: "Old Name");
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "New Name",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Name", existingItem.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAmount()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, amount: 100m);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 500m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(500m, result.Amount);
        Assert.Equal(500m, existingItem.Amount);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesItemType()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, itemType: PlannedItemType.Expense);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Income,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(PlannedItemType.Income, result.ItemType);
        Assert.Equal(PlannedItemType.Income, existingItem.ItemType);
    }

    [Fact]
    public async Task Handle_FixedDateMode_SetsFixedDate()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var fixedDate = new DateOnly(2024, 12, 25);
        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = fixedDate,
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(PlannedItemDateMode.FixedDate, existingItem.DateMode);
        Assert.NotNull(existingItem.FixedDate);
        Assert.Equal(fixedDate, existingItem.FixedDate.FixedDate);
        Assert.Null(existingItem.Schedule);
        Assert.Null(existingItem.FlexibleWindow);
    }

    [Fact]
    public async Task Handle_ScheduleMode_SetsSchedule()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var anchorDate = new DateOnly(2024, 1, 15);
        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Monthly Payment",
            ItemType = PlannedItemType.Expense,
            Amount = 200m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.Schedule,
            ScheduleFrequency = ScheduleFrequency.Monthly,
            ScheduleAnchorDate = anchorDate,
            ScheduleInterval = 1,
            ScheduleDayOfMonth = 15,
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(PlannedItemDateMode.Schedule, existingItem.DateMode);
        Assert.NotNull(existingItem.Schedule);
        Assert.Equal(ScheduleFrequency.Monthly, existingItem.Schedule.Frequency);
        Assert.Equal(anchorDate, existingItem.Schedule.AnchorDate);
        Assert.Equal(1, existingItem.Schedule.Interval);
        Assert.Equal(15, existingItem.Schedule.DayOfMonth);
        Assert.Null(existingItem.FixedDate);
        Assert.Null(existingItem.FlexibleWindow);
    }

    [Fact]
    public async Task Handle_FlexibleWindowMode_SetsWindow()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var windowStart = new DateOnly(2024, 6, 1);
        var windowEnd = new DateOnly(2024, 8, 31);
        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Vacation Fund",
            ItemType = PlannedItemType.Expense,
            Amount = 3000m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FlexibleWindow,
            WindowStartDate = windowStart,
            WindowEndDate = windowEnd,
            AllocationMode = AllocationMode.EvenlySpread,
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(PlannedItemDateMode.FlexibleWindow, existingItem.DateMode);
        Assert.NotNull(existingItem.FlexibleWindow);
        Assert.Equal(windowStart, existingItem.FlexibleWindow.StartDate);
        Assert.Equal(windowEnd, existingItem.FlexibleWindow.EndDate);
        Assert.Equal(AllocationMode.EvenlySpread, existingItem.FlexibleWindow.AllocationMode);
        Assert.Null(existingItem.FixedDate);
        Assert.Null(existingItem.Schedule);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesIsIncluded()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId, isIncluded: true);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = false,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsIncluded);
        Assert.False(existingItem.IsIncluded);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var nonExistentItemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = nonExistentItemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, nonExistentItemId, updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesPlanTimestamp()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var existingItem = TestEntities.CreatePlannedItem(id: itemId, planId: planId);
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, plannedItems: [existingItem]);
        var originalTime = DateTime.UtcNow.AddDays(-1);
        plan.UpdatedUtc = originalTime;

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdatePlannedItemHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateModel = new ModelPlannedItem
        {
            Id = itemId,
            Name = "Test Item",
            ItemType = PlannedItemType.Expense,
            Amount = 100m,
            IsIncluded = true,
            DateMode = PlannedItemDateMode.FixedDate,
            FixedDate = new DateOnly(2024, 6, 15),
        };
        var command = new UpdatePlannedItem(planId, itemId, updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(plan.UpdatedUtc > originalTime);
    }
}
