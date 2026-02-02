#nullable enable
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;

namespace Asm.MooBank.Modules.Budgets.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateLineTests
{
    private readonly TestMocks _mocks;

    public UpdateLineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsBudgetLine()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId, amount: 500m);
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        var updatedLine = TestEntities.CreateBudgetLineModel(id: lineId, tagId: 2, amount: 750m, notes: "Updated");
        var command = new UpdateLine(2024, lineId, updatedLine);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(750m, result.Amount);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId, tagId: 1, amount: 500m, month: 4095);
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        var updatedLine = TestEntities.CreateBudgetLineModel(id: lineId, tagId: 5, amount: 1000m, month: 15, notes: "New notes");
        var command = new UpdateLine(2024, lineId, updatedLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1000m, existingLine.Amount);
        Assert.Equal(5, existingLine.TagId);
        Assert.Equal(15, existingLine.Month);
        Assert.Equal("New notes", existingLine.Notes);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        var updatedLine = TestEntities.CreateBudgetLineModel(id: lineId);
        var command = new UpdateLine(2024, lineId, updatedLine);

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
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        var updatedLine = TestEntities.CreateBudgetLineModel(id: lineId);
        var command = new UpdateLine(2024, lineId, updatedLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentLine_ThrowsException()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(nonExistentId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        var updatedLine = TestEntities.CreateBudgetLineModel(id: nonExistentId);
        var command = new UpdateLine(2024, nonExistentId, updatedLine);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_UpdatesOnlySpecifiedFields()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var existingLine = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId, tagId: 1, amount: 500m, income: true, month: 4095, notes: "Original");
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId, lines: [existingLine]);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetByYear(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        var handler = new UpdateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User, _mocks.SecurityMock.Object);
        // Only update amount
        var updatedLine = TestEntities.CreateBudgetLineModel(id: lineId, tagId: 1, amount: 750m, month: 4095, notes: "Original");
        var command = new UpdateLine(2024, lineId, updatedLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(750m, existingLine.Amount);
        // Note: Income flag is not updated in the handler (it's controlled by Type)
        Assert.True(existingLine.Income);
    }
}
