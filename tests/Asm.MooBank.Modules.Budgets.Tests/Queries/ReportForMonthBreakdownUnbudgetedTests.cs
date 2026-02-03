#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class ReportForMonthBreakdownUnbudgetedTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_ValidBudgetAndMonth_ReturnsUnbudgetedBreakdown()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownUnbudgetedHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

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

        var handler = new ReportForMonthBreakdownUnbudgetedHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_AllTransactionsBudgeted_ReturnsEmptyBreakdown()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var accountId = Guid.NewGuid();

        var rentTag = TestEntities.CreateTag(1, "Rent", familyId);

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

        // Transaction with budgeted tag only
        var rentSplit = TestEntities.CreateTransactionSplit(amount: 950m, tags: [rentTag]);
        var rentTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -950m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit,
            splits: [rentSplit]);

        var transactionQueryable = TestEntities.CreateTransactionQueryable(rentTxn);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownUnbudgetedHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // All transactions are budgeted, so unbudgeted breakdown should be empty
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task Handle_ChildOfBudgetedTag_NotIncludedInUnbudgeted()
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

        // Transaction with child tag (Groceries is child of Food)
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

        var handler = new ReportForMonthBreakdownUnbudgetedHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Groceries is a child of Food (which is budgeted), so should not appear as unbudgeted
        var groceriesItem = result.Tags.FirstOrDefault(i => i.Name == "Groceries");
        Assert.Null(groceriesItem);
    }

    [Fact]
    public async Task Handle_BudgetForDifferentFamily_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var budget = TestEntities.CreateBudget(year: 2024, familyId: otherFamilyId);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable([]);
        var transactionQueryable = TestEntities.CreateTransactionQueryable([]);
        var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthBreakdownUnbudgetedHandler(
            budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
        var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }
}
