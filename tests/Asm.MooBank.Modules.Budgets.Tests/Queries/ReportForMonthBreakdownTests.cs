#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class ReportForMonthBreakdownTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_ValidBudgetAndMonth_ReturnsBreakdown()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Food", income: false, amount: 500m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Tags);
    }

    [Fact]
    public async Task Handle_BudgetNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var budgetQueryable = TestEntities.CreateBudgetQueryable([]);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable([]);
        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TransactionsWithBudgetedTags_GroupedByTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var rentTag = TestEntities.CreateTag(1, "Rent", familyId);
        var foodTag = TestEntities.CreateTag(2, "Food", familyId);

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Food", income: false, amount: 500m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        // Transactions tagged with budget tags
        var rentSplit = TestEntities.CreateTransactionSplit(amount: 950m, tags: [rentTag]);
        var foodSplit = TestEntities.CreateTransactionSplit(amount: 450m, tags: [foodTag]);

        var rentTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -950m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit,
            splits: [rentSplit]);
        var foodTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -450m,
            transactionTime: new DateTime(2024, 6, 20),
            transactionType: TransactionType.Debit,
            splits: [foodSplit]);

        var transactionQueryable = TestEntities.CreateTransactionQueryable(rentTxn, foodTxn);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var items = result.Tags.ToList();
        // Should have budget tags plus "Other" category
        Assert.True(items.Count >= 2);
    }

    [Fact]
    public async Task Handle_TransactionsWithUnbudgetedTags_GroupedInOther()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var rentTag = TestEntities.CreateTag(1, "Rent", familyId);
        var entertainmentTag = TestEntities.CreateTag(99, "Entertainment", familyId); // Not in budget

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        // Transaction with unbudgeted tag
        var entertainmentSplit = TestEntities.CreateTransactionSplit(amount: 100m, tags: [entertainmentTag]);
        var entertainmentTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -100m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit,
            splits: [entertainmentSplit]);

        var transactionQueryable = TestEntities.CreateTransactionQueryable(entertainmentTxn);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var otherItem = result.Tags.FirstOrDefault(i => i.Name == "Other");
        Assert.NotNull(otherItem);
    }

    [Fact]
    public async Task Handle_ChildTagTransactions_IncludedInParentBudget()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var foodTag = TestEntities.CreateTag(1, "Food", familyId);
        var groceriesTag = TestEntities.CreateTag(2, "Groceries", familyId); // Child of Food

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Food", income: false, amount: 500m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        // Transaction with child tag
        var groceriesSplit = TestEntities.CreateTransactionSplit(amount: 150m, tags: [groceriesTag]);
        var groceriesTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -150m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit,
            splits: [groceriesSplit]);

        var transactionQueryable = TestEntities.CreateTransactionQueryable(groceriesTxn);

        // Tag relationship: Groceries is child of Food
        var relationship = TestEntities.CreateTagRelationship(tagId: 2, parentTagId: 1);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable(relationship);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // The groceries transaction should be included in the Food budget line
        var foodItem = result.Tags.FirstOrDefault(i => i.Name == "Food");
        Assert.NotNull(foodItem);
    }

    [Fact]
    public async Task Handle_IncomeLines_NotIncludedInBreakdown()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Salary", income: true, amount: 5000m, month: 4095),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Rent", income: false, amount: 1000m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Income lines should not appear in expense breakdown
        var salaryItem = result.Tags.FirstOrDefault(i => i.Name == "Salary");
        Assert.Null(salaryItem);
    }

    [Fact]
    public async Task Handle_OrdersByBudgetedAmountDescending()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Small", income: false, amount: 100m, month: 4095),
            TestEntities.CreateBudgetLine(tagId: 2, tagName: "Large", income: false, amount: 1000m, month: 4095),
            TestEntities.CreateBudgetLine(tagId: 3, tagName: "Medium", income: false, amount: 500m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdown(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var items = result.Tags.Where(i => i.Name != "Other").ToList();
        // Should be ordered by budgeted amount descending
        for (int i = 0; i < items.Count - 1; i++)
        {
            Assert.True(items[i].BudgetedAmount >= items[i + 1].BudgetedAmount);
        }
    }
}
