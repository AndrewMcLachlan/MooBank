#nullable enable
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Tests.Support;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class RunForecastTests
{
    private readonly TestMocks _mocks;

    public RunForecastTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsForecastResult()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var plans = TestEntities.CreatePlanQueryable(plan);

        var expectedResult = new ForecastResult
        {
            PlanId = planId,
            Months = [],
            Summary = new ForecastSummary
            {
                LowestBalance = 1000m,
                LowestBalanceMonth = new DateOnly(2024, 6, 1),
                RequiredMonthlyUplift = 0m,
                MonthsBelowZero = 0,
                TotalIncome = 60000m,
                TotalOutgoings = 48000m,
                MonthlyBaselineOutgoings = 4000m,
            }
        };

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        _mocks.ForecastEngineMock
            .Setup(e => e.Calculate(It.IsAny<DomainForecastPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(planId);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(planId, result.PlanId);
        Assert.Equal(1000m, result.Summary.LowestBalance);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsForecastEngine()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var plans = TestEntities.CreatePlanQueryable(plan);

        var expectedResult = new ForecastResult
        {
            PlanId = planId,
            Months = [],
            Summary = new ForecastSummary
            {
                LowestBalance = 0m,
                LowestBalanceMonth = DateOnly.MinValue,
                RequiredMonthlyUplift = 0m,
                MonthsBelowZero = 0,
                TotalIncome = 0m,
                TotalOutgoings = 0m,
                MonthlyBaselineOutgoings = 0m,
            }
        };

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        _mocks.ForecastEngineMock
            .Setup(e => e.Calculate(It.IsAny<DomainForecastPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(planId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ForecastEngineMock.Verify(
            e => e.Calculate(It.Is<DomainForecastPlan>(p => p.Id == planId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var plans = TestEntities.CreatePlanQueryable(plan);

        var expectedResult = new ForecastResult
        {
            PlanId = planId,
            Months = [],
            Summary = new ForecastSummary
            {
                LowestBalance = 0m,
                LowestBalanceMonth = DateOnly.MinValue,
                RequiredMonthlyUplift = 0m,
                MonthsBelowZero = 0,
                TotalIncome = 0m,
                TotalOutgoings = 0m,
                MonthlyBaselineOutgoings = 0m,
            }
        };

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        _mocks.ForecastEngineMock
            .Setup(e => e.Calculate(It.IsAny<DomainForecastPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(planId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

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

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(planId);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_NonExistentPlan_ThrowsException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingPlan = TestEntities.CreateForecastPlan(familyId: familyId);
        var nonExistentPlanId = Guid.NewGuid();
        var plans = TestEntities.CreatePlanQueryable(existingPlan);

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(nonExistentPlanId);

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
        Assert.True(
            exception is InvalidOperationException ||
            (exception.InnerException is InvalidOperationException),
            "Expected InvalidOperationException or wrapped InvalidOperationException");
    }

    [Fact]
    public async Task Handle_ForecastEngineReturnsMonths_ReturnsMonthsInResult()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);
        var plans = TestEntities.CreatePlanQueryable(plan);

        var months = new List<ForecastMonth>
        {
            new() { MonthStart = new DateOnly(2024, 1, 1), OpeningBalance = 10000m, IncomeTotal = 0m, BaselineOutgoingsTotal = 0m, PlannedItemsTotal = 0m, ClosingBalance = 8000m },
            new() { MonthStart = new DateOnly(2024, 2, 1), OpeningBalance = 8000m, IncomeTotal = 0m, BaselineOutgoingsTotal = 0m, PlannedItemsTotal = 0m, ClosingBalance = 6000m },
            new() { MonthStart = new DateOnly(2024, 3, 1), OpeningBalance = 6000m, IncomeTotal = 0m, BaselineOutgoingsTotal = 0m, PlannedItemsTotal = 0m, ClosingBalance = 4000m },
        };

        var expectedResult = new ForecastResult
        {
            PlanId = planId,
            Months = months,
            Summary = new ForecastSummary
            {
                LowestBalance = 4000m,
                LowestBalanceMonth = new DateOnly(2024, 3, 1),
                RequiredMonthlyUplift = 0m,
                MonthsBelowZero = 0,
                TotalIncome = 0m,
                TotalOutgoings = 0m,
                MonthlyBaselineOutgoings = 0m,
            }
        };

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        _mocks.ForecastEngineMock
            .Setup(e => e.Calculate(It.IsAny<DomainForecastPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new RunForecastHandler(
            plans,
            _mocks.ForecastEngineMock.Object,
            _mocks.SecurityMock.Object);

        var command = new RunForecast(planId);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Months.Count());
        Assert.Equal(new DateOnly(2024, 1, 1), result.Months.First().MonthStart);
        Assert.Equal(4000m, result.Summary.LowestBalance);
    }
}
