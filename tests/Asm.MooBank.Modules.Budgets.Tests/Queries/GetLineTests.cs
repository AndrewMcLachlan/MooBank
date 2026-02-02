#nullable enable
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class GetLineTests
{
    private readonly TestMocks _mocks;

    public GetLineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingBudgetLine_ReturnsBudgetLine()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId, tagId: 1, tagName: "Groceries", amount: 500m);
        line.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2024, lineId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lineId, result.Id);
        Assert.Equal("Groceries", result.Name);
        Assert.Equal(500m, result.Amount);
    }

    [Fact]
    public async Task Handle_MultipleBudgetLines_ReturnsCorrectOne()
    {
        // Arrange
        var lineId1 = Guid.NewGuid();
        var lineId2 = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line1 = TestEntities.CreateBudgetLine(id: lineId1, budgetId: budgetId, tagId: 1, tagName: "Groceries", amount: 500m);
        var line2 = TestEntities.CreateBudgetLine(id: lineId2, budgetId: budgetId, tagId: 2, tagName: "Rent", amount: 1500m);
        line1.Budget = budget;
        line2.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line1, line2);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId2, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2024, lineId2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(lineId2, result.Id);
        Assert.Equal("Rent", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentBudgetLine_ThrowsNotFoundException()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        line.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(nonExistentId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2024, nonExistentId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WrongYear_ThrowsNotFoundException()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        line.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2023, lineId); // Wrong year

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ChecksSecurity()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId);
        line.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2024, lineId);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IncomeLine_ReturnsCorrectType()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(id: budgetId, year: 2024);
        var line = TestEntities.CreateBudgetLine(id: lineId, budgetId: budgetId, tagId: 1, tagName: "Salary", income: true, amount: 5000m);
        line.Budget = budget;
        var queryable = TestEntities.CreateBudgetLineQueryable(line);

        _mocks.SecurityMock.Setup(s => s.AssertBudgetLinePermission(lineId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new GetLineHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetLine(2024, lineId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(Models.BudgetLineType.Income, result.Type);
    }
}
