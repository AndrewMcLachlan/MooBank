#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
[Collection("DbFunction Tests")]
public class ReportTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_BudgetExists_ReturnsReport()
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

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new Report(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Items.Count()); // 12 months
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

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new Report(2024);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_CreditTransactions_NotIncludedInReport()
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

        // Only credit transaction - should not affect expense report
        var creditTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: 500m,
            transactionTime: new DateTime(2024, 1, 15),
            transactionType: TransactionType.Credit);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(creditTxn);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new Report(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var januaryItem = result.Items.First(i => i.Month == 1);
        Assert.Equal(0m, januaryItem.Actual); // Credit not included
    }

    [Fact]
    public async Task Handle_AccountNotInBudget_TransactionsNotIncluded()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var budgetAccountId = Guid.NewGuid();
        var nonBudgetAccountId = Guid.NewGuid();

        var lines = new[]
        {
            TestEntities.CreateBudgetLine(tagId: 1, tagName: "Rent", income: false, amount: 1000m, month: 4095),
        };
        var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
        var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

        var budgetAccount = TestEntities.CreateLogicalAccount(id: budgetAccountId, includeInBudget: true);
        var nonBudgetAccount = TestEntities.CreateLogicalAccount(id: nonBudgetAccountId, includeInBudget: false);
        var accountQueryable = TestEntities.CreateLogicalAccountQueryable(budgetAccount, nonBudgetAccount);

        // Transaction from non-budget account
        var txn = TestEntities.CreateTransaction(
            accountId: nonBudgetAccountId,
            amount: -500m,
            transactionTime: new DateTime(2024, 1, 15),
            transactionType: TransactionType.Debit);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(txn);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [budgetAccountId, nonBudgetAccountId]));

        var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new Report(2024);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        var januaryItem = result.Items.First(i => i.Month == 1);
        Assert.Equal(0m, januaryItem.Actual); // Non-budget account not included
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

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new Report(2024);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_DebitTransactions_CalculatesNetAmountCorrectly()
    {
        // Arrange - Set up the delegate to simulate net amount calculation
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
        {
            // Simulate: debit transactions return negative net amounts
            return type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount);
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Expenses", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            var txn1 = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -300m,
                transactionTime: new DateTime(2024, 1, 10),
                transactionType: TransactionType.Debit);
            var txn2 = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -200m,
                transactionTime: new DateTime(2024, 1, 20),
                transactionType: TransactionType.Debit);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(txn1, txn2);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new Report(2024);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var januaryItem = result.Items.First(i => i.Month == 1);
            Assert.Equal(500m, januaryItem.Actual); // Sum of net amounts: |-300| + |-200| = 500
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_TransactionsWithOffsets_UsesNetAmountNotGrossAmount()
    {
        // Arrange - Simulate transactions with partial offsets
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
        {
            // Simulate: the -500 transaction has a 100 offset, so net is -400
            if (Math.Abs(amount) == 500m) return -400m;
            return type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount);
        });

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Expenses", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            // Transaction with gross amount 500, but net amount is 400 due to offset
            var txn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -500m,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(txn);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new Report(2024);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var marchItem = result.Items.First(i => i.Month == 3);
            Assert.Equal(400m, marchItem.Actual); // Net amount (400), not gross amount (500)
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_TransactionsAcrossMonths_GroupsByMonthCorrectly()
    {
        // Arrange
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
            type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount));

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Expenses", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            var janTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -100m,
                transactionTime: new DateTime(2024, 1, 15),
                transactionType: TransactionType.Debit);
            var febTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -200m,
                transactionTime: new DateTime(2024, 2, 15),
                transactionType: TransactionType.Debit);
            var marchTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -300m,
                transactionTime: new DateTime(2024, 3, 15),
                transactionType: TransactionType.Debit);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(janTxn, febTxn, marchTxn);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new Report(2024);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100m, result.Items.First(i => i.Month == 1).Actual);
            Assert.Equal(200m, result.Items.First(i => i.Month == 2).Actual);
            Assert.Equal(300m, result.Items.First(i => i.Month == 3).Actual);
            Assert.Equal(0m, result.Items.First(i => i.Month == 4).Actual); // No transactions
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_ExcludedFromReporting_NotIncludedInTotals()
    {
        // Arrange
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
            type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount));

        try
        {
            var familyId = _mocks.User.FamilyId;
            var accountId = Guid.NewGuid();

            var lines = new[]
            {
                TestEntities.CreateBudgetLine(tagId: 1, tagName: "Expenses", income: false, amount: 1000m, month: 4095),
            };
            var budget = TestEntities.CreateBudget(year: 2024, familyId: familyId, lines: lines);
            var budgetQueryable = TestEntities.CreateBudgetQueryable(budget);

            var account = TestEntities.CreateLogicalAccount(id: accountId, includeInBudget: true);
            var accountQueryable = TestEntities.CreateLogicalAccountQueryable(account);

            var includedTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -100m,
                transactionTime: new DateTime(2024, 1, 15),
                transactionType: TransactionType.Debit,
                excludeFromReporting: false);
            var excludedTxn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -500m,
                transactionTime: new DateTime(2024, 1, 20),
                transactionType: TransactionType.Debit,
                excludeFromReporting: true);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(includedTxn, excludedTxn);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new Report(2024);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            var januaryItem = result.Items.First(i => i.Month == 1);
            Assert.Equal(100m, januaryItem.Actual); // Only the non-excluded transaction
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }
}
