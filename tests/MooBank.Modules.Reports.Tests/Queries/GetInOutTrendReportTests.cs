#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetInOutTrendReportTests
{
    private static readonly Guid TestAccountId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ValidQuery_ReturnsReportWithAccountAndDates()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var transactions = CreateTransactionQueryable([]);

        var handler = new GetInOutTrendReportHandler(transactions);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = start,
            End = end,
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestAccountId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_EmptyTransactions_ReturnsEmptyIncomeAndExpenses()
    {
        // Arrange
        var transactions = CreateTransactionQueryable([]);

        var handler = new GetInOutTrendReportHandler(transactions);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Income);
        Assert.Empty(result.Expenses);
    }

    [Fact]
    public async Task Handle_CreditTransactions_GroupsAsIncome()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, DateTime.Today.AddDays(-5), TransactionType.Credit),
            CreateTransaction(TestAccountId, 500m, DateTime.Today.AddDays(-3), TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(result.Income);
        Assert.Empty(result.Expenses);
        Assert.Equal(1500m, result.Income.Sum(i => i.GrossAmount));
    }

    [Fact]
    public async Task Handle_DebitTransactions_GroupsAsExpenses()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -200m, DateTime.Today.AddDays(-5), TransactionType.Debit),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-3), TransactionType.Debit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Income);
        Assert.NotEmpty(result.Expenses);
        // NetAmount is calculated, so we check the absolute sum
        var totalExpenses = result.Expenses.Sum(e => Math.Abs(e.GrossAmount));
        Assert.Equal(300m, totalExpenses);
    }

    [Fact]
    public async Task Handle_MixedTransactions_SeparatesIncomeAndExpenses()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, DateTime.Today.AddDays(-10), TransactionType.Credit),
            CreateTransaction(TestAccountId, -200m, DateTime.Today.AddDays(-5), TransactionType.Debit),
            CreateTransaction(TestAccountId, 500m, DateTime.Today.AddDays(-3), TransactionType.Credit),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-2), TransactionType.Debit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(result.Income);
        Assert.NotEmpty(result.Expenses);
    }

    [Fact]
    public async Task Handle_GroupsTransactionsByMonth()
    {
        // Arrange - transactions in different months
        var thisMonth = DateTime.Today;
        var lastMonth = DateTime.Today.AddMonths(-1);

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, thisMonth, TransactionType.Credit),
            CreateTransaction(TestAccountId, 500m, thisMonth.AddDays(-5), TransactionType.Credit),
            CreateTransaction(TestAccountId, 800m, lastMonth, TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Income.Count()); // Two months of data
    }

    [Fact]
    public async Task Handle_OrdersTrendPointsByMonth()
    {
        // Arrange - transactions in different months (added out of order)
        var month1 = new DateTime(DateTime.Today.Year, 1, 15);
        var month2 = new DateTime(DateTime.Today.Year, 2, 15);
        var month3 = new DateTime(DateTime.Today.Year, 3, 15);

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 300m, month3, TransactionType.Credit),
            CreateTransaction(TestAccountId, 100m, month1, TransactionType.Credit),
            CreateTransaction(TestAccountId, 200m, month2, TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(month1.AddDays(-10)),
            End = DateOnly.FromDateTime(month3.AddDays(10)),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var incomeList = result.Income.ToList();
        Assert.Equal(3, incomeList.Count);
        Assert.True(incomeList[0].Month < incomeList[1].Month);
        Assert.True(incomeList[1].Month < incomeList[2].Month);
    }

    [Fact]
    public async Task Handle_SumsTransactionsWithinSameMonth()
    {
        // Arrange - multiple transactions in the same month
        // Use a fixed month (15th) to avoid date boundary issues
        var baseDate = new DateTime(2024, 6, 15);

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, baseDate, TransactionType.Credit),
            CreateTransaction(TestAccountId, 500m, baseDate.AddDays(5), TransactionType.Credit),
            CreateTransaction(TestAccountId, 250m, baseDate.AddDays(10), TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(baseDate.AddDays(-10)),
            End = DateOnly.FromDateTime(baseDate.AddDays(15)),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var incomeList = result.Income.ToList();
        Assert.Single(incomeList);
        Assert.Equal(1750m, incomeList[0].GrossAmount);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByAccountId()
    {
        // Arrange
        var otherAccountId = Guid.NewGuid();

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, DateTime.Today.AddDays(-5), TransactionType.Credit),
            CreateTransaction(otherAccountId, 5000m, DateTime.Today.AddDays(-5), TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Income);
        Assert.Equal(1000m, result.Income.First().GrossAmount);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByDateRange()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, DateTime.Today.AddDays(-5), TransactionType.Credit),   // In range
            CreateTransaction(TestAccountId, 5000m, DateTime.Today.AddMonths(-6), TransactionType.Credit), // Out of range
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Income);
        Assert.Equal(1000m, result.Income.First().GrossAmount);
    }

    [Fact]
    public async Task Handle_ExcludesTransactionsMarkedExcludeFromReporting()
    {
        // Arrange
        var includedTransaction = CreateTransaction(TestAccountId, 1000m, DateTime.Today.AddDays(-5), TransactionType.Credit);
        var excludedTransaction = CreateTransaction(TestAccountId, 5000m, DateTime.Today.AddDays(-5), TransactionType.Credit);
        excludedTransaction.ExcludeFromReporting = true;

        var transactions = new[] { includedTransaction, excludedTransaction };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Income);
        Assert.Equal(1000m, result.Income.First().GrossAmount);
    }

    [Fact]
    public async Task Handle_TrendPointHasCorrectMonth()
    {
        // Arrange
        var transactionDate = new DateTime(2024, 6, 15); // June 15th

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 1000m, transactionDate, TransactionType.Credit),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetInOutTrendReportHandler(transactionsQueryable);

        var query = new GetInOutTrendReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(transactionDate.AddMonths(-1)),
            End = DateOnly.FromDateTime(transactionDate.AddMonths(1)),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var incomePoint = result.Income.First();
        Assert.Equal(new DateOnly(2024, 6, 1), incomePoint.Month); // First of the month
    }

    #region Helper Methods

    private static DomainTransaction CreateTransaction(
        Guid accountId,
        decimal amount,
        DateTime transactionTime,
        TransactionType transactionType)
    {
        var transactionId = Guid.NewGuid();
        var transaction = new DomainTransaction(transactionId)
        {
            AccountId = accountId,
            Amount = amount,
            TransactionTime = transactionTime,
            TransactionType = transactionType,
            Source = "Test",
            ExcludeFromReporting = false,
        };

        // Create a default split
        var split = new DomainTransactionSplit(Guid.NewGuid())
        {
            TransactionId = transactionId,
            Amount = Math.Abs(amount),
        };

        // Use reflection to add the split to the private _splits list
        var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var splitsList = (List<DomainTransactionSplit>)splitsField!.GetValue(transaction)!;
        splitsList.Add(split);

        return transaction;
    }

    private static IQueryable<DomainTransaction> CreateTransactionQueryable(IEnumerable<DomainTransaction> transactions)
    {
        return QueryableHelper.CreateAsyncQueryable(transactions);
    }

    #endregion
}
