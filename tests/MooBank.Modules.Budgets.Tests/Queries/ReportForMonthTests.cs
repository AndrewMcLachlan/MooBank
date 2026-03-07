#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
[Collection("DbFunction Tests")]
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

    [Fact]
    public async Task Handle_DebitTransactions_SumsNetAmountsCorrectly()
    {
        // Arrange - Set up the delegate to calculate net amounts
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

            var txn1 = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -150m,
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
            Assert.Equal(400m, result.Actual); // |(-150) + (-250)| = 400
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_TransactionsWithOffsets_UsesNetAmount()
    {
        // Arrange - Simulate transactions with offsets
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
        {
            // Simulate: -600 transaction has 150 offset, net is -450
            if (Math.Abs(amount) == 600m) return -450m;
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

            // Gross amount 600, net amount 450 due to 150 offset
            var txn = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -600m,
                transactionTime: new DateTime(2024, 6, 15),
                transactionType: TransactionType.Debit);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(txn);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new ReportForMonth(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(450m, result.Actual); // Net amount, not gross
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }

    [Fact]
    public async Task Handle_MultipleTransactionsWithVaryingOffsets_SumsNetAmounts()
    {
        // Arrange - Different transactions have different offset scenarios
        DomainTransaction.SetTransactionNetAmountOverride((type, id, amount) =>
        {
            // Simulate varying offset scenarios
            return Math.Abs(amount) switch
            {
                100m => -80m,   // 20 offset
                200m => -200m,  // No offset
                300m => -150m,  // 150 offset (50% rebate)
                _ => type == TransactionType.Debit ? -Math.Abs(amount) : Math.Abs(amount)
            };
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
                amount: -100m,
                transactionTime: new DateTime(2024, 6, 5),
                transactionType: TransactionType.Debit);
            var txn2 = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -200m,
                transactionTime: new DateTime(2024, 6, 15),
                transactionType: TransactionType.Debit);
            var txn3 = TestEntities.CreateTransaction(
                accountId: accountId,
                amount: -300m,
                transactionTime: new DateTime(2024, 6, 25),
                transactionType: TransactionType.Debit);
            var transactionQueryable = TestEntities.CreateTransactionQueryable(txn1, txn2, txn3);

            _mocks.SetUser(TestMocks.CreateTestUser(familyId: familyId, accounts: [accountId]));

            var handler = new ReportForMonthHandler(budgetQueryable, accountQueryable, transactionQueryable, _mocks.User);
            var query = new ReportForMonth(2024, 6);

            // Act
            var result = await handler.Handle(query, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            // Sum of net amounts: |(-80) + (-200) + (-150)| = 430
            Assert.Equal(430m, result.Actual);
        }
        finally
        {
            DomainTransaction.ResetTransactionNetAmountOverride();
        }
    }
}
