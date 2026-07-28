#nullable enable
using Asm.MooBank.Modules.Retirement.Commands;
using Asm.MooBank.Modules.Retirement.Tests.Support;
using DomainPlan = Asm.MooBank.Domain.Entities.Retirement.RetirementPlan;

namespace Asm.MooBank.Modules.Retirement.Tests.Commands;

/// <summary>
/// Unit tests for deleting a retirement plan.
/// </summary>
[Trait("Category", "Unit")]
public class DeletePlanTests
{
    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given an existing plan
    /// When it is deleted
    /// Then it should be removed from the repository and the change saved
    /// </summary>
    /// <remarks>
    /// A retirement plan is deleted outright rather than archived like a forecast plan: it holds
    /// assumptions rather than a record of anything that happened, so there is nothing to keep.
    /// </remarks>
    [Fact]
    public async Task Handle_ExistingPlan_IsDeletedAndSaved()
    {
        // Arrange
        var plan = TestEntities.CreatePlan();
        _mocks.RetirementRepositoryMock
            .Setup(r => r.Get(plan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var handler = new DeletePlanHandler(_mocks.RetirementRepositoryMock.Object, _mocks.UnitOfWorkMock.Object);

        // Act
        await handler.Handle(new DeletePlan(plan.Id), TestContext.Current.CancellationToken);

        // Assert
        _mocks.RetirementRepositoryMock.Verify(r => r.Delete(plan), Times.Once);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
