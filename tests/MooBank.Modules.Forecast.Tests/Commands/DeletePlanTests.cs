#nullable enable
using Asm.MooBank.Modules.Forecast.Commands;
using Asm.MooBank.Modules.Forecast.Tests.Support;

namespace Asm.MooBank.Modules.Forecast.Tests.Commands;

[Trait("Category", "Unit")]
public class DeletePlanTests
{
    private readonly TestMocks _mocks;

    public DeletePlanTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ArchivesPlan()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId, isArchived: false);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new DeletePlan(planId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(plan.IsArchived);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var planId = Guid.NewGuid();
        var plan = TestEntities.CreateForecastPlan(id: planId, familyId: familyId);

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new DeletePlan(planId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}
