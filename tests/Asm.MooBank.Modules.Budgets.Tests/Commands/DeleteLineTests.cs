#nullable enable
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Commands;

[Trait("Category", "Unit")]
public class DeleteLineTests
{
    private readonly TestMocks _mocks;

    public DeleteLineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesLine()
    {
        // Arrange
        var lineId = Guid.NewGuid();

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock.Setup(r => r.DeleteLine(lineId));

        var handler = new DeleteLineHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.SecurityMock.Object);
        var command = new DeleteLine(2024, lineId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.BudgetRepositoryMock.Verify(r => r.DeleteLine(lineId), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var lineId = Guid.NewGuid();

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock.Setup(r => r.DeleteLine(lineId));

        var handler = new DeleteLineHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.SecurityMock.Object);
        var command = new DeleteLine(2024, lineId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ChecksSecurity()
    {
        // Arrange
        var lineId = Guid.NewGuid();

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock.Setup(r => r.DeleteLine(lineId));

        var handler = new DeleteLineHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.SecurityMock.Object);
        var command = new DeleteLine(2024, lineId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SecurityCheckBeforeDelete()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var callOrder = new List<string>();

        _mocks.SecurityMock
            .Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("security"))
            .Returns(Task.CompletedTask);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.DeleteLine(lineId))
            .Callback(() => callOrder.Add("delete"));

        var handler = new DeleteLineHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.SecurityMock.Object);
        var command = new DeleteLine(2024, lineId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, callOrder.Count);
        Assert.Equal("security", callOrder[0]);
        Assert.Equal("delete", callOrder[1]);
    }

    [Fact]
    public async Task Handle_MultipleDeletes_EachDeletesCorrectLine()
    {
        // Arrange
        var lineId1 = Guid.NewGuid();
        var lineId2 = Guid.NewGuid();

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock.Setup(r => r.DeleteLine(It.IsAny<Guid>()));

        var handler = new DeleteLineHandler(_mocks.BudgetRepositoryMock.Object, _mocks.UnitOfWorkMock.Object, _mocks.SecurityMock.Object);

        // Act
        await handler.Handle(new DeleteLine(2024, lineId1), CancellationToken.None);
        await handler.Handle(new DeleteLine(2024, lineId2), CancellationToken.None);

        // Assert
        _mocks.BudgetRepositoryMock.Verify(r => r.DeleteLine(lineId1), Times.Once);
        _mocks.BudgetRepositoryMock.Verify(r => r.DeleteLine(lineId2), Times.Once);
    }
}
