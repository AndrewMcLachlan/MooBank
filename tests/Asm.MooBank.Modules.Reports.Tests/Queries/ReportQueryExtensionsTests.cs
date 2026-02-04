#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class ReportQueryExtensionsTests
{
    private static readonly Guid TestAccountId = Guid.NewGuid();
    private static readonly Guid OtherAccountId = Guid.NewGuid();

    #region WhereByReportQuery Basic Filtering

    [Fact]
    public void WhereByReportQuery_FiltersCorrectAccountId()
    {
        // Arrange
        var transactions = CreateTransactions(
            CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5)),
            CreateTransaction(OtherAccountId, 200m, DateTime.Today.AddDays(-5))
        );
        var query = CreateReportQuery(TestAccountId);

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(TestAccountId, result[0].AccountId);
    }

    [Fact]
    public void WhereByReportQuery_ExcludesExcludedFromReporting()
    {
        // Arrange
        var included = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var excluded = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-5));
        excluded.ExcludeFromReporting = true;

        var transactions = CreateTransactions(included, excluded);
        var query = CreateReportQuery(TestAccountId);

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].Amount);
    }

    [Fact]
    public void WhereByReportQuery_FiltersTransactionsAfterEndDate()
    {
        // Arrange
        var inRange = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var afterEnd = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(5));

        var transactions = CreateTransactions(inRange, afterEnd);
        var query = CreateReportQuery(TestAccountId, DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].Amount);
    }

    [Fact]
    public void WhereByReportQuery_FiltersTransactionsBeforeStartDate()
    {
        // Arrange
        var inRange = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var beforeStart = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddMonths(-2));

        var transactions = CreateTransactions(inRange, beforeStart);
        var query = CreateReportQuery(TestAccountId, DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].Amount);
    }

    #endregion

    #region WhereByReportQuery Start Date MinValue Branch

    [Fact]
    public void WhereByReportQuery_StartDateMinValue_IncludesAllHistoricTransactions()
    {
        // Arrange - Using MinValue for start date should include all transactions
        var recent = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var veryOld = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddYears(-10));

        var transactions = CreateTransactions(recent, veryOld);
        var query = CreateReportQuery(TestAccountId, DateOnly.MinValue, DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void WhereByReportQuery_StartDateMinValue_StillFiltersEndDate()
    {
        // Arrange - MinValue start should still respect end date
        var inRange = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var afterEnd = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(5));

        var transactions = CreateTransactions(inRange, afterEnd);
        var query = CreateReportQuery(TestAccountId, DateOnly.MinValue, DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void WhereByReportQuery_NonMinValueStartDate_FiltersOlderTransactions()
    {
        // Arrange - Non-MinValue start should filter old transactions
        var recent = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5));
        var veryOld = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddYears(-10));

        var transactions = CreateTransactions(recent, veryOld);
        var query = CreateReportQuery(TestAccountId, DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].Amount);
    }

    #endregion

    #region WhereByReportQuery Boundary Conditions

    [Fact]
    public void WhereByReportQuery_TransactionOnStartDate_IsIncluded()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var onStartDate = CreateTransaction(TestAccountId, 100m, startDate.ToDateTime(TimeOnly.MinValue));

        var transactions = CreateTransactions(onStartDate);
        var query = CreateReportQuery(TestAccountId, startDate, DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void WhereByReportQuery_TransactionOnEndDate_IsIncluded()
    {
        // Arrange
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        var onEndDate = CreateTransaction(TestAccountId, 100m, endDate.ToDateTime(new TimeOnly(12, 0)));

        var transactions = CreateTransactions(onEndDate);
        var query = CreateReportQuery(TestAccountId, DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), endDate);

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void WhereByReportQuery_TransactionAtEndOfEndDate_IsIncluded()
    {
        // Arrange - Transaction at 23:59:59 on end date should be included
        var endDate = DateOnly.FromDateTime(DateTime.Today);
        var atEndOfDay = CreateTransaction(TestAccountId, 100m, endDate.ToDateTime(new TimeOnly(23, 59, 59)));

        var transactions = CreateTransactions(atEndOfDay);
        var query = CreateReportQuery(TestAccountId, DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), endDate);

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region ExcludeOffset Tests

    [Fact]
    public void ExcludeOffset_TransactionsWithNoOffset_AllIncluded()
    {
        // Arrange
        var transaction1 = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-1));
        var transaction2 = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-2));

        var transactions = CreateTransactions(transaction1, transaction2);

        // Act
        var result = transactions.ExcludeOffset().ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    // Note: Tests for ExcludeOffset with actual offsets would require complex entity setup
    // The extension method is tested implicitly through integration tests

    #endregion

    #region TypedReportQuery WhereByReportQuery Tests

    [Fact]
    public void TypedWhereByReportQuery_FiltersAccountAndType()
    {
        // Arrange
        var creditTransaction = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5), TransactionType.Credit);
        var debitTransaction = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-5), TransactionType.Debit);

        var transactions = CreateTransactions(creditTransaction, debitTransaction);
        var query = CreateTypedReportQuery(TestAccountId, TestEntities.CreateDebitReportType());

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(TransactionType.Debit, result[0].TransactionType);
    }

    [Fact]
    public void TypedWhereByReportQuery_CreditType_OnlyIncludesCredits()
    {
        // Arrange
        var creditTransaction = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5), TransactionType.Credit);
        var debitTransaction = CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-5), TransactionType.Debit);

        var transactions = CreateTransactions(creditTransaction, debitTransaction);
        var query = CreateTypedReportQuery(TestAccountId, TestEntities.CreateCreditReportType());

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(TransactionType.Credit, result[0].TransactionType);
    }

    [Fact]
    public void TypedWhereByReportQuery_CombinesAllFilters()
    {
        // Arrange - Mix of transactions that should test all filters
        var matchAll = CreateTransaction(TestAccountId, 100m, DateTime.Today.AddDays(-5), TransactionType.Debit);
        var wrongAccount = CreateTransaction(OtherAccountId, 200m, DateTime.Today.AddDays(-5), TransactionType.Debit);
        var wrongType = CreateTransaction(TestAccountId, 300m, DateTime.Today.AddDays(-5), TransactionType.Credit);
        var wrongDate = CreateTransaction(TestAccountId, 400m, DateTime.Today.AddMonths(-3), TransactionType.Debit);
        var excluded = CreateTransaction(TestAccountId, 500m, DateTime.Today.AddDays(-5), TransactionType.Debit);
        excluded.ExcludeFromReporting = true;

        var transactions = CreateTransactions(matchAll, wrongAccount, wrongType, wrongDate, excluded);
        var query = CreateTypedReportQuery(TestAccountId, TestEntities.CreateDebitReportType(),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)), DateOnly.FromDateTime(DateTime.Today));

        // Act
        var result = transactions.WhereByReportQuery(query).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].Amount);
    }

    #endregion

    #region Helper Methods

    private static IQueryable<DomainTransaction> CreateTransactions(params DomainTransaction[] transactions)
    {
        return QueryableHelper.CreateAsyncQueryable(transactions);
    }

    private static DomainTransaction CreateTransaction(
        Guid accountId,
        decimal amount,
        DateTime transactionTime,
        TransactionType transactionType = TransactionType.Debit)
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

        // Add a default split
        var split = new DomainTransactionSplit(Guid.NewGuid())
        {
            TransactionId = transactionId,
            Amount = Math.Abs(amount),
        };

        var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var splitsList = (List<DomainTransactionSplit>)splitsField!.GetValue(transaction)!;
        splitsList.Add(split);

        return transaction;
    }

    private static TestReportQuery CreateReportQuery(
        Guid accountId,
        DateOnly? start = null,
        DateOnly? end = null)
    {
        return new TestReportQuery
        {
            AccountId = accountId,
            Start = start ?? DateOnly.MinValue,
            End = end ?? DateOnly.MaxValue,
        };
    }

    private static TestTypedReportQuery CreateTypedReportQuery(
        Guid accountId,
        Asm.MooBank.Modules.Reports.Models.ReportType reportType,
        DateOnly? start = null,
        DateOnly? end = null)
    {
        return new TestTypedReportQuery
        {
            AccountId = accountId,
            ReportType = reportType,
            Start = start ?? DateOnly.MinValue,
            End = end ?? DateOnly.MaxValue,
        };
    }

    private record TestReportQuery : ReportQuery;

    private record TestTypedReportQuery : TypedReportQuery;

    #endregion
}
