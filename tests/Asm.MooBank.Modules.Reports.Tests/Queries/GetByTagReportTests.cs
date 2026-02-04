#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using Asm.MooBank.Modules.Reports.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainTransactionSplit = Asm.MooBank.Domain.Entities.Transactions.TransactionSplit;

namespace Asm.MooBank.Modules.Reports.Tests.Queries;

[Trait("Category", "Unit")]
public class GetByTagReportTests
{
    private static readonly Guid TestAccountId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ValidQuery_ReturnsReportWithAccountAndDates()
    {
        // Arrange
        var start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
        var end = DateOnly.FromDateTime(DateTime.Today);
        var transactions = CreateTransactionQueryable([]);

        var handler = new GetByTagReportHandler(transactions);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = start,
            End = end,
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestAccountId, result.AccountId);
        Assert.Equal(start, result.Start);
        Assert.Equal(end, result.End);
    }

    [Fact]
    public async Task Handle_TransactionsWithTags_GroupsByTagAndSumsAmounts()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries");
        var tag2 = CreateTag(2, "Utilities");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, -50m, DateTime.Today.AddDays(-3), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, -75m, DateTime.Today.AddDays(-2), TransactionType.Debit, [tag2]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var tagsList = result.Tags.ToList();
        Assert.Equal(3, tagsList.Count); // Groceries, Utilities, and Untagged

        var groceriesTag = tagsList.FirstOrDefault(t => t.TagName == "Groceries");
        Assert.NotNull(groceriesTag);
        Assert.Equal(150m, groceriesTag.GrossAmount); // 100 + 50

        var utilitiesTag = tagsList.FirstOrDefault(t => t.TagName == "Utilities");
        Assert.NotNull(utilitiesTag);
        Assert.Equal(75m, utilitiesTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_EmptyTransactions_ReturnsOnlyUntaggedWithZero()
    {
        // Arrange
        var transactions = CreateTransactionQueryable([]);

        var handler = new GetByTagReportHandler(transactions);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var tagsList = result.Tags.ToList();
        Assert.Single(tagsList);
        Assert.Equal("Untagged", tagsList[0].TagName);
        Assert.Equal(0m, tagsList[0].GrossAmount);
    }

    [Fact]
    public async Task Handle_UntaggedTransactions_IncludesUntaggedCategory()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, -50m, DateTime.Today.AddDays(-3), TransactionType.Debit, []), // Untagged
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var untagged = result.Tags.FirstOrDefault(t => t.TagName == "Untagged");
        Assert.NotNull(untagged);
        Assert.Equal(50m, untagged.GrossAmount);
    }

    [Fact]
    public async Task Handle_WithParentTagId_DoesNotIncludeUntaggedCategory()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
            ParentTagId = 1,
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var untagged = result.Tags.FirstOrDefault(t => t.TagName == "Untagged");
        Assert.Null(untagged);
    }

    [Fact]
    public async Task Handle_TransactionWithMultipleTags_CountsForEachTag()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries");
        var tag2 = CreateTag(2, "Food");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1, tag2]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var groceriesTag = result.Tags.FirstOrDefault(t => t.TagName == "Groceries");
        var foodTag = result.Tags.FirstOrDefault(t => t.TagName == "Food");

        Assert.NotNull(groceriesTag);
        Assert.NotNull(foodTag);
        Assert.Equal(100m, groceriesTag.GrossAmount);
        Assert.Equal(100m, foodTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_OrdersByGrossAmountDescending()
    {
        // Arrange
        var tag1 = CreateTag(1, "Small");
        var tag2 = CreateTag(2, "Medium");
        var tag3 = CreateTag(3, "Large");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -50m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-4), TransactionType.Debit, [tag2]),
            CreateTransaction(TestAccountId, -200m, DateTime.Today.AddDays(-3), TransactionType.Debit, [tag3]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var tagsList = result.Tags.ToList();
        Assert.Equal("Large", tagsList[0].TagName);
        Assert.Equal("Medium", tagsList[1].TagName);
        Assert.Equal("Small", tagsList[2].TagName);
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByDateRange()
    {
        // Arrange
        var tag1 = CreateTag(1, "InRange");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),  // In range
            CreateTransaction(TestAccountId, -200m, DateTime.Today.AddMonths(-2), TransactionType.Debit, [tag1]), // Out of range
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var inRangeTag = result.Tags.FirstOrDefault(t => t.TagName == "InRange");
        Assert.NotNull(inRangeTag);
        Assert.Equal(100m, inRangeTag.GrossAmount); // Only the in-range transaction
    }

    [Fact]
    public async Task Handle_FiltersTransactionsByAccountId()
    {
        // Arrange
        var otherAccountId = Guid.NewGuid();
        var tag1 = CreateTag(1, "TestTag");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(otherAccountId, -200m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount); // Only from TestAccountId
    }

    [Fact]
    public async Task Handle_DebitReportType_OnlyIncludesDebitTransactions()
    {
        // Arrange
        var tag1 = CreateTag(1, "TestTag");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-5), TransactionType.Credit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount); // Only the debit transaction
    }

    [Fact]
    public async Task Handle_CreditReportType_OnlyIncludesCreditTransactions()
    {
        // Arrange
        var tag1 = CreateTag(1, "TestTag");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, 200m, DateTime.Today.AddDays(-5), TransactionType.Credit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateCreditReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(200m, testTag.GrossAmount); // Only the credit transaction
    }

    [Fact]
    public async Task Handle_ExcludesTransactionsMarkedExcludeFromReporting()
    {
        // Arrange
        var tag1 = CreateTag(1, "TestTag");

        var includedTransaction = CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]);
        var excludedTransaction = CreateTransaction(TestAccountId, -200m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]);
        excludedTransaction.ExcludeFromReporting = true;

        var transactions = new[] { includedTransaction, excludedTransaction };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount); // Only the non-excluded transaction
    }

    #region Helper Methods

    private static DomainTag CreateTag(int id, string name)
    {
        return new DomainTag(id)
        {
            Name = name,
            FamilyId = Guid.NewGuid(),
        };
    }

    private static DomainTransaction CreateTransaction(
        Guid accountId,
        decimal amount,
        DateTime transactionTime,
        TransactionType transactionType,
        IEnumerable<DomainTag> tags)
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

        // Create a split with the tags
        var split = new DomainTransactionSplit(Guid.NewGuid())
        {
            TransactionId = transactionId,
            Amount = Math.Abs(amount),
            Tags = [.. tags],
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
