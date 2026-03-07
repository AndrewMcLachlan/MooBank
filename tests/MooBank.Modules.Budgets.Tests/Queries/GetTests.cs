#nullable enable
using Asm.MooBank.Modules.Budgets.Commands;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingBudget_ReturnsBudget()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Salary", income: true, amount: 5000m),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Groceries", income: false, amount: 500m),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Year);
        Assert.Single(result.IncomeLines);
        Assert.Single(result.ExpensesLines);
    }

    [Fact]
    public async Task Handle_ExistingBudget_ReturnsCorrectMonths()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Salary", income: true, amount: 3000m, month: 4095), // All months
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Rent", income: false, amount: 1500m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Months.Count());
    }

    [Fact]
    public async Task Handle_NonExistentBudget_CreatesBudget()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var queryable = TestEntities.CreateBudgetQueryable([]);

        var createdBudget = new Models.Budget
        {
            Year = 2024,
            IncomeLines = [],
            ExpensesLines = [],
            Months = [],
        };

        _mocks.CommandDispatcherMock
            .Setup(d => d.Dispatch(It.IsAny<Create>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBudget);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Year);
        _mocks.CommandDispatcherMock.Verify(d => d.Dispatch(It.Is<Create>(c => c.Year == 2024), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleBudgets_ReturnsCorrectYear()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var budget2023 = TestEntities.CreateBudget(year: 2023, familyId: familyId);
        var budget2024 = TestEntities.CreateBudget(year: 2024, familyId: familyId);
        var budget2025 = TestEntities.CreateBudget(year: 2025, familyId: familyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget2023, budget2024, budget2025);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.Year);
    }

    [Fact]
    public async Task Handle_BudgetForDifferentFamily_CreatesBudgetForCurrentUser()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        var budget = TestEntities.CreateBudget(year: 2024, familyId: otherFamilyId);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var createdBudget = new Models.Budget
        {
            Year = 2024,
            IncomeLines = [],
            ExpensesLines = [],
            Months = [],
        };

        _mocks.CommandDispatcherMock
            .Setup(d => d.Dispatch(It.IsAny<Create>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBudget);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.CommandDispatcherMock.Verify(d => d.Dispatch(It.IsAny<Create>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BudgetWithOnlyIncome_ReturnsEmptyExpenses()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Salary", income: true, amount: 5000m),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var queryable = TestEntities.CreateBudgetQueryable(budget);

        var handler = new GetHandler(queryable, _mocks.User, _mocks.CommandDispatcherMock.Object);
        var query = new Get(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.IncomeLines);
        Assert.Empty(result.ExpensesLines);
    }
}
