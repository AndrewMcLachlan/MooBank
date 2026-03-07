#nullable enable
using Asm.MooBank.Modules.Budgets.Models;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;

namespace Asm.MooBank.Modules.Budgets.Tests.ModelExtensions;

[Trait("Category", "Unit")]
public class BudgetExtensionsTests
{
    #region ToModel Tests

    [Fact]
    public void ToModel_NullBudget_ReturnsNull()
    {
        // Arrange
        DomainBudget? budget = null;

        // Act
        var result = budget.ToModel();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToModel_EmptyBudget_ReturnsEmptyCollections()
    {
        // Arrange
        var budget = TestEntities.CreateBudget(year: 2024);

        // Act
        var result = budget.ToModel();

        // Assert
        Assert.Equal(2024, result.Year);
        Assert.Empty(result.IncomeLines);
        Assert.Empty(result.ExpensesLines);
        Assert.Equal(12, result.Months.Count());
    }

    [Fact]
    public void ToModel_WithIncomeLines_SeparatesIncomeAndExpenses()
    {
        // Arrange
        var incomeLine = TestEntities.CreateBudgetLine(
            tagId: 1, tagName: "Salary", amount: 5000m, income: true, month: 4095);
        var expenseLine = TestEntities.CreateBudgetLine(
            tagId: 2, tagName: "Rent", amount: 1500m, income: false, month: 4095);

        var budget = TestEntities.CreateBudget(lines: [incomeLine, expenseLine]);

        // Act
        var result = budget.ToModel();

        // Assert
        Assert.Single(result.IncomeLines);
        Assert.Single(result.ExpensesLines);
        Assert.Equal("Salary", result.IncomeLines.First().Name);
        Assert.Equal("Rent", result.ExpensesLines.First().Name);
    }

    [Fact]
    public void ToModel_WithMonthlyAmounts_CalculatesMonthTotals()
    {
        // Arrange - Line that applies to all months (4095 = all 12 bits set)
        var incomeLine = TestEntities.CreateBudgetLine(
            tagId: 1, tagName: "Salary", amount: 1000m, income: true, month: 4095);
        var expenseLine = TestEntities.CreateBudgetLine(
            tagId: 2, tagName: "Rent", amount: 500m, income: false, month: 4095);

        var budget = TestEntities.CreateBudget(lines: [incomeLine, expenseLine]);

        // Act
        var result = budget.ToModel();

        // Assert - Each month should have the same income/expenses
        foreach (var month in result.Months)
        {
            Assert.Equal(1000m, month.Income);
            Assert.Equal(500m, month.Expenses);
        }
    }

    [Fact]
    public void ToModel_WithSpecificMonths_OnlyAppliesAmountToThoseMonths()
    {
        // Arrange - Line that only applies to January (bit 0)
        var januaryOnlyLine = TestEntities.CreateBudgetLine(
            tagId: 1, tagName: "Annual Fee", amount: 1200m, income: false, month: 1); // Only January

        var budget = TestEntities.CreateBudget(lines: [januaryOnlyLine]);

        // Act
        var result = budget.ToModel();

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(1200m, months[0].Expenses); // January
        Assert.Equal(0m, months[1].Expenses); // February
        Assert.Equal(0m, months[11].Expenses); // December
    }

    [Fact]
    public void ToModel_WithQuarterlyMonth_AppliesTo4Months()
    {
        // Arrange - Line that applies to Jan, Apr, Jul, Oct (quarterly)
        // Bit positions: 0, 3, 6, 9 → 1 + 8 + 64 + 512 = 585
        var quarterlyLine = TestEntities.CreateBudgetLine(
            tagId: 1, tagName: "Quarterly Bill", amount: 300m, income: false, month: 585);

        var budget = TestEntities.CreateBudget(lines: [quarterlyLine]);

        // Act
        var result = budget.ToModel();

        // Assert
        var months = result.Months.ToList();
        Assert.Equal(300m, months[0].Expenses); // January
        Assert.Equal(0m, months[1].Expenses); // February
        Assert.Equal(0m, months[2].Expenses); // March
        Assert.Equal(300m, months[3].Expenses); // April
        Assert.Equal(300m, months[6].Expenses); // July
        Assert.Equal(300m, months[9].Expenses); // October
    }

    [Fact]
    public void ToModel_OrdersLinesByName()
    {
        // Arrange
        var lineZ = TestEntities.CreateBudgetLine(tagId: 1, tagName: "Zebra", amount: 100m, income: false);
        var lineA = TestEntities.CreateBudgetLine(tagId: 2, tagName: "Apple", amount: 200m, income: false);
        var lineM = TestEntities.CreateBudgetLine(tagId: 3, tagName: "Mango", amount: 300m, income: false);

        var budget = TestEntities.CreateBudget(lines: [lineZ, lineA, lineM]);

        // Act
        var result = budget.ToModel();

        // Assert
        var names = result.ExpensesLines.Select(l => l.Name).ToList();
        Assert.Equal(["Apple", "Mango", "Zebra"], names);
    }

    #endregion

    #region ToMonths Tests

    [Fact]
    public void ToMonths_EmptyBudget_Returns12ZeroMonths()
    {
        // Arrange
        var budget = TestEntities.CreateBudget();

        // Act
        var result = budget.ToMonths();

        // Assert
        Assert.Equal(12, result.Count());
        Assert.All(result, m =>
        {
            Assert.Equal(0m, m.Income);
            Assert.Equal(0m, m.Expenses);
        });
    }

    [Fact]
    public void ToMonths_UsesOneBasedMonthNumber()
    {
        // Arrange
        var budget = TestEntities.CreateBudget();

        // Act
        var result = budget.ToMonths().ToList();

        // Assert - Month numbers should be 1-12, not 0-11
        Assert.Equal(1, result[0].Month);
        Assert.Equal(12, result[11].Month);
    }

    [Fact]
    public void ToMonths_WithAllMonthsLine_CalculatesCorrectTotals()
    {
        // Arrange
        var incomeLine = TestEntities.CreateBudgetLine(amount: 2000m, income: true, month: 4095);
        var expenseLine = TestEntities.CreateBudgetLine(amount: 1500m, income: false, month: 4095);

        var budget = TestEntities.CreateBudget(lines: [incomeLine, expenseLine]);

        // Act
        var result = budget.ToMonths().ToList();

        // Assert
        foreach (var month in result)
        {
            Assert.Equal(2000m, month.Income);
            Assert.Equal(1500m, month.Expenses);
        }
    }

    [Fact]
    public void ToMonths_WithDecemberOnlyLine_AppliesOnlyToDecember()
    {
        // Arrange - December is bit 11 → 2048
        var decemberLine = TestEntities.CreateBudgetLine(
            tagId: 1, tagName: "Christmas", amount: 500m, income: false, month: 2048);

        var budget = TestEntities.CreateBudget(lines: [decemberLine]);

        // Act
        var result = budget.ToMonths().ToList();

        // Assert
        for (int i = 0; i < 11; i++)
        {
            Assert.Equal(0m, result[i].Expenses);
        }
        Assert.Equal(500m, result[11].Expenses); // December (month 12, index 11)
    }

    #endregion

    #region WhereMonth Tests

    [Fact]
    public void WhereMonth_JanuaryOnly_ReturnsJanuaryLines()
    {
        // Arrange
        var januaryLine = TestEntities.CreateBudgetLine(tagId: 1, tagName: "Jan Only", month: 1);
        var allMonthsLine = TestEntities.CreateBudgetLine(tagId: 2, tagName: "All Months", month: 4095);
        var februaryLine = TestEntities.CreateBudgetLine(tagId: 3, tagName: "Feb Only", month: 2);

        var lines = new[] { januaryLine, allMonthsLine, februaryLine };

        // Act
        var result = lines.WhereMonth(1).ToList(); // Month 1 = January

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.Tag.Name == "Jan Only");
        Assert.Contains(result, l => l.Tag.Name == "All Months");
        Assert.DoesNotContain(result, l => l.Tag.Name == "Feb Only");
    }

    [Fact]
    public void WhereMonth_DecemberOnly_ReturnsDecemberLines()
    {
        // Arrange
        var decemberLine = TestEntities.CreateBudgetLine(tagId: 1, tagName: "Dec Only", month: 2048);
        var novemberLine = TestEntities.CreateBudgetLine(tagId: 2, tagName: "Nov Only", month: 1024);

        var lines = new[] { decemberLine, novemberLine };

        // Act
        var result = lines.WhereMonth(12).ToList(); // Month 12 = December

        // Assert
        Assert.Single(result);
        Assert.Equal("Dec Only", result[0].Tag.Name);
    }

    [Fact]
    public void WhereMonth_JuneFromAllMonths_ReturnsMatch()
    {
        // Arrange
        var allMonthsLine = TestEntities.CreateBudgetLine(tagId: 1, tagName: "Monthly", month: 4095);

        // Act
        var result = new[] { allMonthsLine }.WhereMonth(6).ToList(); // June

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void WhereMonth_EmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var lines = Array.Empty<DomainBudgetLine>();

        // Act
        var result = lines.WhereMonth(1);

        // Assert
        Assert.Empty(result);
    }

    #endregion
}

[Trait("Category", "Unit")]
public class BudgetLineExtensionsTests
{
    [Fact]
    public void ToModel_IncomeLine_SetsTypeToIncome()
    {
        // Arrange
        var line = TestEntities.CreateBudgetLine(income: true, amount: 1000m);

        // Act
        var result = line.ToModel();

        // Assert
        Assert.Equal(BudgetLineType.Income, result.Type);
    }

    [Fact]
    public void ToModel_ExpenseLine_SetsTypeToExpenses()
    {
        // Arrange
        var line = TestEntities.CreateBudgetLine(income: false, amount: 500m);

        // Act
        var result = line.ToModel();

        // Assert
        Assert.Equal(BudgetLineType.Expenses, result.Type);
    }

    [Fact]
    public void ToModel_MapsAllProperties()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var line = TestEntities.CreateBudgetLine(
            id: lineId,
            tagId: 42,
            tagName: "Groceries",
            notes: "Weekly shopping",
            amount: 250.50m,
            income: false,
            month: 15); // Jan, Feb, Mar, Apr

        // Act
        var result = line.ToModel();

        // Assert
        Assert.Equal(lineId, result.Id);
        Assert.Equal(42, result.TagId);
        Assert.Equal("Groceries", result.Name);
        Assert.Equal("Weekly shopping", result.Notes);
        Assert.Equal(250.50m, result.Amount);
        Assert.Equal(15, result.Month);
        Assert.Equal(BudgetLineType.Expenses, result.Type);
    }

    [Fact]
    public void ToDomain_IncomeLine_SetsIncomeToTrue()
    {
        // Arrange
        var line = TestEntities.CreateBudgetLineModel(type: BudgetLineType.Income);
        var budgetId = Guid.NewGuid();

        // Act
        var result = line.ToDomain(budgetId);

        // Assert
        Assert.True(result.Income);
    }

    [Fact]
    public void ToDomain_ExpenseLine_SetsIncomeToFalse()
    {
        // Arrange
        var line = TestEntities.CreateBudgetLineModel(type: BudgetLineType.Expenses);
        var budgetId = Guid.NewGuid();

        // Act
        var result = line.ToDomain(budgetId);

        // Assert
        Assert.False(result.Income);
    }

    [Fact]
    public void ToDomain_MapsAllProperties()
    {
        // Arrange
        var budgetId = Guid.NewGuid();
        var line = TestEntities.CreateBudgetLineModel(
            tagId: 99,
            notes: "Test notes",
            amount: 750m,
            month: 2048); // December only

        // Act
        var result = line.ToDomain(budgetId);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(budgetId, result.BudgetId);
        Assert.Equal(99, result.TagId);
        Assert.Equal("Test notes", result.Notes);
        Assert.Equal(750m, result.Amount);
        Assert.Equal(2048, result.Month);
    }

    [Fact]
    public void ToModel_Collection_MapsAllItems()
    {
        // Arrange
        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "First"),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Second"),
            TestEntities.CreateBudgetLine(tagId: 3, tagName: "Third"),
        };

        // Act
        var result = lines.ToModel().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("First", result[0].Name);
        Assert.Equal("Second", result[1].Name);
        Assert.Equal("Third", result[2].Name);
    }
}
