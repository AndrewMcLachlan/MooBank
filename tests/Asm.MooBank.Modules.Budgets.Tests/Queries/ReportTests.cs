#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
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
}
