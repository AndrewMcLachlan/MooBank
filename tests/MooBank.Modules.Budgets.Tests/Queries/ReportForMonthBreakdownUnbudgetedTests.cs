#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
[Collection("DbFunction Tests")]
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

    [Fact]
    public async Task Handle_UnbudgetedTransactions_CalculatesSplitNetAmounts()
    {
        // Arrange - Set up the delegate to calculate split net amounts
        DomainTransactionSplit.SetTransactionSplitNetAmountOverride((txnId, splitId, amount) =>
        {
            // Return the amount as-is (no offsets)
            return amount;
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            // Budgeted tag
            var rentTag = TestEntities.CreateTag(1, "Rent", familyId);
            // Unbudgeted tag
            var coffeeTag = TestEntities.CreateTag(2, "Coffee", familyId);

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            // Transaction with unbudgeted tag
            var coffeeSplit = TestEntities.CreateTransactionSplit(amount: 25m, tags: [coffeeTag]);
            var coffeeTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -25m,
                transactionTime: new DateTime(2024, 6, 15),
                transactionType: TransactionType.Debit,
                splits: [coffeeSplit]);

            var transactionQueryable = TestEntities.CreateTransactionQueryable(coffeeTxn);
            var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthBreakdownUnbudgetedHandler(
                budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
            var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var coffeeItem = result.Tags.FirstOrDefault(i => i.Name == "Coffee");
            Assert.NotNull(coffeeItem);
            Assert.Equal(25m, coffeeItem.Actual);
        }
        finally
        {
            DomainTransactionSplit.ResetTransactionSplitNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_SplitsWithOffsets_UsesNetAmountNotGrossAmount()
    {
        // Arrange - Simulate splits with partial offsets
        DomainTransactionSplit.SetTransactionSplitNetAmountOverride((txnId, splitId, amount) =>
        {
            // Simulate: 200 amount has 50 offset, net is 150
            if (amount == 200m) return 150m;
            return amount;
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var entertainmentTag = TestEntities.CreateTag(3, "Entertainment", familyId);

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            // Gross amount 200, but net is 150 due to offset
            var entertainmentSplit = TestEntities.CreateTransactionSplit(amount: 200m, tags: [entertainmentTag]);
            var entertainmentTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -200m,
                transactionTime: new DateTime(2024, 6, 20),
                transactionType: TransactionType.Debit,
                splits: [entertainmentSplit]);

            var transactionQueryable = TestEntities.CreateTransactionQueryable(entertainmentTxn);
            var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthBreakdownUnbudgetedHandler(
                budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
            var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var entertainmentItem = result.Tags.FirstOrDefault(i => i.Name == "Entertainment");
            Assert.NotNull(entertainmentItem);
            Assert.Equal(150m, entertainmentItem.Actual); // Net amount, not gross
        }
        finally
        {
            DomainTransactionSplit.ResetTransactionSplitNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_MultipleUnbudgetedTags_SumsNetAmountsPerTag()
    {
        // Arrange
        DomainTransactionSplit.SetTransactionSplitNetAmountOverride((txnId, splitId, amount) =>
        {
            // Apply varying offsets based on amount
            return amount switch
            {
                50m => 40m,   // 10 offset
                75m => 75m,   // No offset
                100m => 80m,  // 20 offset
                _ => amount
            };
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var coffeeTag = TestEntities.CreateTag(2, "Coffee", familyId);
            var entertainmentTag = TestEntities.CreateTag(3, "Entertainment", familyId);

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            // Multiple transactions with unbudgeted tags
            var coffee1Split = TestEntities.CreateTransactionSplit(amount: 50m, tags: [coffeeTag]);
            var coffee1Txn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -50m,
                transactionTime: new DateTime(2024, 6, 5),
                transactionType: TransactionType.Debit,
                splits: [coffee1Split]);

            var coffee2Split = TestEntities.CreateTransactionSplit(amount: 75m, tags: [coffeeTag]);
            var coffee2Txn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -75m,
                transactionTime: new DateTime(2024, 6, 15),
                transactionType: TransactionType.Debit,
                splits: [coffee2Split]);

            var entertainmentSplit = TestEntities.CreateTransactionSplit(amount: 100m, tags: [entertainmentTag]);
            var entertainmentTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -100m,
                transactionTime: new DateTime(2024, 6, 25),
                transactionType: TransactionType.Debit,
                splits: [entertainmentSplit]);

            var transactionQueryable = TestEntities.CreateTransactionQueryable(coffee1Txn, coffee2Txn, entertainmentTxn);
            var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthBreakdownUnbudgetedHandler(
                budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
            var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);

            var coffeeItem = result.Tags.FirstOrDefault(i => i.Name == "Coffee");
            Assert.NotNull(coffeeItem);
            Assert.Equal(115m, coffeeItem.Actual); // 40 + 75 = 115

            var entertainmentItem = result.Tags.FirstOrDefault(i => i.Name == "Entertainment");
            Assert.NotNull(entertainmentItem);
            Assert.Equal(80m, entertainmentItem.Actual);
        }
        finally
        {
            DomainTransactionSplit.ResetTransactionSplitNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_SplitFullyOffset_ReturnsZeroNetAmount()
    {
        // Arrange - Split is fully offset (e.g., refund received)
        DomainTransactionSplit.SetTransactionSplitNetAmountOverride((txnId, splitId, amount) =>
        {
            // Simulate full offset - net amount is zero
            return 0m;
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var refundedTag = TestEntities.CreateTag(4, "Refunded Purchase", familyId);

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            // Transaction that was fully refunded
            var refundedSplit = TestEntities.CreateTransactionSplit(amount: 500m, tags: [refundedTag]);
            var refundedTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -500m,
                transactionTime: new DateTime(2024, 6, 10),
                transactionType: TransactionType.Debit,
                splits: [refundedSplit]);

            var transactionQueryable = TestEntities.CreateTransactionQueryable(refundedTxn);
            var tagRelationshipQueryable = TestEntities.CreateTagRelationshipQueryable([]);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthBreakdownUnbudgetedHandler(
                budgetQueryable, accountQueryable, transactionQueryable, tagRelationshipQueryable, _mocks.User);
            var query = new ReportForMonthBreakdownUnbudgeted(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var refundedItem = result.Tags.FirstOrDefault(i => i.Name == "Refunded Purchase");
            Assert.NotNull(refundedItem);
            Assert.Equal(0m, refundedItem.Actual); // Fully offset
        }
        finally
        {
            DomainTransactionSplit.ResetTransactionSplitNetAmountOverride();
        }
    }
}
