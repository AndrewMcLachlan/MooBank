#nullable enable
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdatePlanTests
{
    private readonly TestMocks _mocks;

    public UpdatePlanTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesPlanName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var existingPlan = TestEntities.CreateForecastPlan(
            id: planId,
            name: "Old Name",
            familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "New Name",
            StartDate = existingPlan.StartDate,
            EndDate = existingPlan.EndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = existingPlan.StartingBalanceMode,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Name", existingPlan.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesDates()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var existingPlan = TestEntities.CreateForecastPlan(
            id: planId,
            familyId: familyId,
            startDate: new DateOnly(2024, 1, 1),
            endDate: new DateOnly(2024, 12, 31));

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var newStartDate = new DateOnly(2024, 6, 1);
        var newEndDate = new DateOnly(2025, 5, 31);
        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "Test Plan",
            StartDate = newStartDate,
            EndDate = newEndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = existingPlan.StartingBalanceMode,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(newStartDate, result.StartDate);
        Assert.Equal(newEndDate, result.EndDate);
        Assert.Equal(newStartDate, existingPlan.StartDate);
        Assert.Equal(newEndDate, existingPlan.EndDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesBalanceSettings()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var existingPlan = TestEntities.CreateForecastPlan(
            id: planId,
            familyId: familyId,
            startingBalanceMode: StartingBalanceMode.ManualAmount,
            startingBalanceAmount: 1000m);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "Test Plan",
            StartDate = existingPlan.StartDate,
            EndDate = existingPlan.EndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = StartingBalanceMode.CalculatedCurrent,
            StartingBalanceAmount = 5000m,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(StartingBalanceMode.CalculatedCurrent, result.StartingBalanceMode);
        Assert.Equal(5000m, result.StartingBalanceAmount);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesStrategies()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var existingPlan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        // Whatever the plan was carrying before the update.
        existingPlan.IncomeStrategySerialized = """{"manualRecurring":{"amount":12960}}""";

        var outgoingStrategy = new OutgoingStrategy
        {
            LookbackMonths = 6
        };

        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "Test Plan",
            StartDate = existingPlan.StartDate,
            EndDate = existingPlan.EndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = existingPlan.StartingBalanceMode,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
            OutgoingStrategy = outgoingStrategy,
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(existingPlan.OutgoingStrategySerialized);
        Assert.Contains("6", existingPlan.OutgoingStrategySerialized);

        // The income strategy is no longer part of the model, but the column still holds the figures
        // the migration turns into planned income items. An update must leave them alone: writing
        // null here would destroy them before the migration ever runs.
        Assert.Contains("12960", existingPlan.IncomeStrategySerialized);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var existingPlan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "Test Plan",
            StartDate = existingPlan.StartDate,
            EndDate = existingPlan.EndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = existingPlan.StartingBalanceMode,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesTimestamp()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var originalTime = DateTime.UtcNow.AddDays(-1);
        var existingPlan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        existingPlan.UpdatedUtc = originalTime;

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<ForecastPlanDetailsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPlan);

        var handler = new UpdatePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateModel = new ForecastPlan
        {
            Id = planId,
            Name = "Test Plan",
            StartDate = existingPlan.StartDate,
            EndDate = existingPlan.EndDate,
            AccountScopeMode = existingPlan.AccountScopeMode,
            StartingBalanceMode = existingPlan.StartingBalanceMode,
            IsArchived = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            AccountIds = [],
            PlannedItems = [],
        };
        var command = new UpdatePlan(planId, updateModel);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(existingPlan.UpdatedUtc > originalTime);
    }
}
