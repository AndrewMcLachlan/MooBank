#nullable enable
using Asm.MooBank.Modules.Budgets.Queries;
using Asm.MooBank.Modules.Budgets.Tests.Support;

namespace Asm.MooBank.Modules.Budgets.Tests.Queries;

[Trait("Category", "Unit")]
public class GetValueForTagTests
{
    private readonly TestMocks _mocks = new();

    [Fact]
    public async Task Handle_TransactionsWithTag_ReturnsSumOfAmounts()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 5;
        var tag = TestEntities.CreateTag(tagId, "Groceries");

        var split1 = TestEntities.CreateTransactionSplit(amount: 50m, tags: [tag]);
        var split2 = TestEntities.CreateTransactionSplit(amount: 75m, tags: [tag]);

        var txn1 = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -50m,
            transactionTime: new DateTime(2024, 6, 15),
            splits: [split1]);
        var txn2 = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -75m,
            transactionTime: new DateTime(2024, 6, 20),
            splits: [split2]);

        var queryable = TestEntities.CreateTransactionQueryable(txn1, txn2);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(-125m, result); // -50 + -75
    }

    [Fact]
    public async Task Handle_NoTransactionsWithTag_ReturnsZero()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 5;
        var otherTag = TestEntities.CreateTag(99, "Other");

        var split = TestEntities.CreateTransactionSplit(amount: 50m, tags: [otherTag]);
        var txn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -50m,
            transactionTime: new DateTime(2024, 6, 15),
            splits: [split]);

        var queryable = TestEntities.CreateTransactionQueryable(txn);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task Handle_TransactionsOutsideDateRange_ReturnsLastTransactionAmount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 5;
        var tag = TestEntities.CreateTag(tagId, "Groceries");

        var split = TestEntities.CreateTransactionSplit(amount: 100m, tags: [tag]);
        var txn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -100m,
            transactionTime: new DateTime(2024, 5, 15), // Before date range
            splits: [split]);

        var queryable = TestEntities.CreateTransactionQueryable(txn);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(-100m, result); // Falls back to most recent transaction amount
    }

    [Fact]
    public async Task Handle_TransactionsFromOtherAccounts_NotIncluded()
    {
        // Arrange
        var userAccountId = Guid.NewGuid();
        var otherAccountId = Guid.NewGuid();
        var tagId = 5;
        var tag = TestEntities.CreateTag(tagId, "Groceries");

        var userSplit = TestEntities.CreateTransactionSplit(amount: 50m, tags: [tag]);
        var otherSplit = TestEntities.CreateTransactionSplit(amount: 200m, tags: [tag]);

        var userTxn = TestEntities.CreateTransaction(
            accountId: userAccountId,
            amount: -50m,
            transactionTime: new DateTime(2024, 6, 15),
            splits: [userSplit]);
        var otherTxn = TestEntities.CreateTransaction(
            accountId: otherAccountId,
            amount: -200m,
            transactionTime: new DateTime(2024, 6, 15),
            splits: [otherSplit]);

        var queryable = TestEntities.CreateTransactionQueryable(userTxn, otherTxn);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [userAccountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(-50m, result); // Only user's account transactions
    }

    [Fact]
    public async Task Handle_ExcludedFromReportingTransactions_NotIncluded()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 5;
        var tag = TestEntities.CreateTag(tagId, "Groceries");

        var includedSplit = TestEntities.CreateTransactionSplit(amount: 50m, tags: [tag]);
        var excludedSplit = TestEntities.CreateTransactionSplit(amount: 100m, tags: [tag]);

        var includedTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -50m,
            transactionTime: new DateTime(2024, 6, 15),
            excludeFromReporting: false,
            splits: [includedSplit]);
        var excludedTxn = TestEntities.CreateTransaction(
            accountId: accountId,
            amount: -100m,
            transactionTime: new DateTime(2024, 6, 20),
            excludeFromReporting: true,
            splits: [excludedSplit]);

        var queryable = TestEntities.CreateTransactionQueryable(includedTxn, excludedTxn);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(-50m, result); // Only non-excluded transactions
    }

    [Fact]
    public async Task Handle_EmptyTransactions_ReturnsZero()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var tagId = 5;

        var queryable = TestEntities.CreateTransactionQueryable([]);

        _mocks.SetUser(TestMocks.CreateTestUser(accounts: [accountId]));

        var handler = new GetValueForTagHandler(queryable, _mocks.User);
        var query = new GetValueForTag(tagId)
        {
            Start = new DateOnly(2024, 6, 1),
            End = new DateOnly(2024, 6, 30),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(0m, result);
    }
}
