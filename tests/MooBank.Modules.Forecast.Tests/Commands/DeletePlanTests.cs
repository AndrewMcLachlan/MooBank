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

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

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

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new DeletePlan(planId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

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
            .Setup(r => r.Get(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new DeletePlan(planId);

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

        _mocks.ForecastRepositoryMock
            .Setup(r => r.Get(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new DeletePlanHandler(
            _mocks.ForecastRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new DeletePlan(planId);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
