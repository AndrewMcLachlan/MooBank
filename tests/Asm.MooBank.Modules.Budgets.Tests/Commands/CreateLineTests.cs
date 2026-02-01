#nullable enable
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;

namespace Asm.MooBank.Modules.Budgets.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateLineTests
{
    private readonly TestMocks _mocks;

    public CreateLineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsBudgetLine()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Groceries");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel(tagId: 1, name: "Groceries", amount: 500m);
        var command = new CreateLine(2024, budgetLine);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500m, result.Amount);
        Assert.Equal(1, result.TagId);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsLineToRepository()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Rent");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel(tagId: 2, name: "Rent", amount: 1500m);
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.BudgetRepositoryMock.Verify(r => r.AddLine(It.IsAny<DomainBudgetLine>()), Times.Once);
        Assert.NotNull(capturedLine);
        Assert.Equal(budgetId, capturedLine.BudgetId);
        Assert.Equal(1500m, capturedLine.Amount);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Test");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel();
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IncomeLine_SetsIncomeTrue()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Salary");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel(tagId: 1, name: "Salary", amount: 5000m, type: Models.BudgetLineType.Income);
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLine);
        Assert.True(capturedLine.Income);
    }

    [Fact]
    public async Task Handle_ExpenseLine_SetsIncomeFalse()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Groceries");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel(tagId: 1, name: "Groceries", amount: 500m, type: Models.BudgetLineType.Expenses);
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLine);
        Assert.False(capturedLine.Income);
    }

    [Fact]
    public async Task Handle_WithNotes_SetsNotes()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Test");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        var budgetLine = TestEntities.CreateBudgetLineModel(notes: "Weekly grocery budget");
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLine);
        Assert.Equal("Weekly grocery budget", capturedLine.Notes);
    }

    [Fact]
    public async Task Handle_WithMonthMask_SetsMonth()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024, familyId: _mocks.User.FamilyId);

        _mocks.BudgetRepositoryMock
            .Setup(r => r.GetOrCreate(_mocks.User.FamilyId, (short)2024, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        DomainBudgetLine? capturedLine = null;
        _mocks.BudgetRepositoryMock
            .Setup(r => r.AddLine(It.IsAny<DomainBudgetLine>()))
            .Callback<DomainBudgetLine>(l => capturedLine = l)
            .Returns<DomainBudgetLine>(l =>
            {
                l.Tag = TestEntities.CreateTag(l.TagId, "Test");
                return l;
            });

        var handler = new CreateLineHandler(_mocks.UnitOfWorkMock.Object, _mocks.BudgetRepositoryMock.Object, _mocks.User);
        // 0b000000001111 = January through April only
        var budgetLine = TestEntities.CreateBudgetLineModel(month: 15);
        var command = new CreateLine(2024, budgetLine);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLine);
        Assert.Equal(15, capturedLine.Month);
    }
}
