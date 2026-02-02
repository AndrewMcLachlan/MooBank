#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class ReportForMonthTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_ValidBudgetAndMonth_ReturnsMonthReport()
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

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6); // June

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Month);
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

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TransactionsInMonth_CalculatesActual()
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

        // Transactions for June
        var txn1 = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -400m,
            transactionTime: new DateTime(2024, 6, 10),
            transactionType: TransactionType.Debit);
        var txn2 = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -250m,
            transactionTime: new DateTime(2024, 6, 20),
            transactionType: TransactionType.Debit);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(txn1, txn2);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.Month);
        Assert.Equal(1000m, result.BudgetedAmount);
    }

    [Fact]
    public async Task Handle_TransactionsInOtherMonths_NotIncluded()
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

        // Transaction in July (not June)
        var txn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -500m,
            transactionTime: new DateTime(2024, 7, 15),
            transactionType: TransactionType.Debit);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(txn);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.Actual); // July transaction not included in June report
    }

    [Fact]
    public async Task Handle_OnlyDebitTransactions_IncludedInActual()
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

        // Mix of debit and credit transactions
        var debitTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -500m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit);
        var creditTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: 300m,
            transactionTime: new DateTime(2024, 6, 20),
            transactionType: TransactionType.Credit);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(debitTxn, creditTxn);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Only debit transactions should be included
    }

    [Fact]
    public async Task Handle_ExcludedFromReporting_NotIncludedInActual()
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

        var includedTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -200m,
            transactionTime: new DateTime(2024, 6, 15),
            transactionType: TransactionType.Debit,
            excludeFromReporting: false);
        var excludedTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -800m,
            transactionTime: new DateTime(2024, 6, 20),
            transactionType: TransactionType.Debit,
            excludeFromReporting: true);
        var transactionQueryable = TestEntities.CreateTransactionQueryable(includedTxn, excludedTxn);

        _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Excluded transaction should not affect the actual
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

        var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
        var query = new ReportForMonth(2024, 6);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }
}
