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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount); // Only the non-excluded transaction
    }

    [Fact]
    public async Task Handle_SameTagInDifferentTagCombinations_AggregatesCorrectly()
    {
        // Arrange - Tests the double-grouping logic where same tag appears in different tag combinations
        var tagA = CreateTag(1, "Common");
        var tagB = CreateTag(2, "GroupB");
        var tagC = CreateTag(3, "GroupC");

        // Transaction 1: Common + GroupB
        // Transaction 2: Common + GroupC
        // Both should contribute to Common's total
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tagA, tagB]),
            CreateTransaction(TestAccountId, -50m, DateTime.Today.AddDays(-4), TransactionType.Debit, [tagA, tagC]),
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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - Common tag should sum both transactions (100 + 50 = 150)
        var commonTag = result.Tags.FirstOrDefault(t => t.TagName == "Common");
        Assert.NotNull(commonTag);
        Assert.Equal(150m, commonTag.GrossAmount);

        // GroupB and GroupC should each have their individual amounts
        var groupBTag = result.Tags.FirstOrDefault(t => t.TagName == "GroupB");
        Assert.NotNull(groupBTag);
        Assert.Equal(100m, groupBTag.GrossAmount);

        var groupCTag = result.Tags.FirstOrDefault(t => t.TagName == "GroupC");
        Assert.NotNull(groupCTag);
        Assert.Equal(50m, groupCTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_TransactionAtStartOfDayBoundary_IsIncluded()
    {
        // Arrange - Test ToStartOfDay boundary
        var tag1 = CreateTag(1, "TestTag");
        var startDate = DateTime.Today.AddDays(-10);

        // Transaction at exactly 00:00:00 on start date should be included
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, startDate.Date, TransactionType.Debit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(startDate),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_TransactionAtEndOfDayBoundary_IsIncluded()
    {
        // Arrange - Test ToEndOfDay boundary
        var tag1 = CreateTag(1, "TestTag");
        var endDate = DateTime.Today;

        // Transaction at 23:59:59 on end date should be included
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59), TransactionType.Debit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(endDate),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(100m, testTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_WithMinValueStartDate_IncludesAllHistoricalTransactions()
    {
        // Arrange - Tests the DateOnly.MinValue special case in ReportQuery
        var tag1 = CreateTag(1, "OldTag");

        // Very old transaction
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, new DateTime(2010, 1, 1), TransactionType.Debit, [tag1]),
            CreateTransaction(TestAccountId, -50m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
        };
        var transactionsQueryable = CreateTransactionQueryable(transactions);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.MinValue, // No start date filter
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - Should include both old and recent transactions
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "OldTag");
        Assert.NotNull(testTag);
        Assert.Equal(150m, testTag.GrossAmount); // 100 + 50
    }

    [Fact]
    public async Task Handle_NegativeNetAmounts_ConvertsToPositiveGrossAmount()
    {
        // Arrange - Tests Math.Abs on NetAmount
        var tag1 = CreateTag(1, "TestTag");

        // Debit transaction has negative amount
        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -250m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - GrossAmount should be positive (absolute value)
        var testTag = result.Tags.FirstOrDefault(t => t.TagName == "TestTag");
        Assert.NotNull(testTag);
        Assert.Equal(250m, testTag.GrossAmount);
        Assert.True(testTag.GrossAmount > 0);
    }

    [Fact]
    public async Task Handle_ZeroAmountTransaction_IncludedWithZeroValue()
    {
        // Arrange - Edge case: zero amount transaction
        var tag1 = CreateTag(1, "ZeroTag");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, 0m, DateTime.Today.AddDays(-5), TransactionType.Debit, [tag1]),
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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var zeroTag = result.Tags.FirstOrDefault(t => t.TagName == "ZeroTag");
        Assert.NotNull(zeroTag);
        Assert.Equal(0m, zeroTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_TransactionWithMultipleSplits_AggregatesAllSplitTags()
    {
        // Arrange - Transaction with multiple splits, each with different tags
        var tag1 = CreateTag(1, "Split1Tag");
        var tag2 = CreateTag(2, "Split2Tag");

        var transaction = CreateTransactionWithMultipleSplits(
            TestAccountId,
            -200m,
            DateTime.Today.AddDays(-5),
            TransactionType.Debit,
            [(100m, [tag1]), (100m, [tag2])]);

        var transactionsQueryable = CreateTransactionQueryable([transaction]);

        var handler = new GetByTagReportHandler(transactionsQueryable);

        var query = new GetByTagReport
        {
            AccountId = TestAccountId,
            Start = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            End = DateOnly.FromDateTime(DateTime.Today),
            ReportType = TestEntities.CreateDebitReportType(),
        };

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - Both tags should appear with transaction net amount
        var split1Tag = result.Tags.FirstOrDefault(t => t.TagName == "Split1Tag");
        var split2Tag = result.Tags.FirstOrDefault(t => t.TagName == "Split2Tag");

        Assert.NotNull(split1Tag);
        Assert.NotNull(split2Tag);
    }

    [Fact]
    public async Task Handle_LargeNumberOfTags_HandlesCorrectly()
    {
        // Arrange - Many tags to test performance and correctness
        var tags = Enumerable.Range(1, 20).Select(i => CreateTag(i, $"Tag{i}")).ToList();

        var transactions = tags.Select((tag, i) =>
            CreateTransaction(TestAccountId, -(i + 1) * 10m, DateTime.Today.AddDays(-i - 1), TransactionType.Debit, [tag])
        ).ToArray();

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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - Should have 21 tags (20 + Untagged)
        Assert.Equal(21, result.Tags.Count());

        // First tag should be Tag20 with highest amount (200)
        var firstTag = result.Tags.First();
        Assert.Equal("Tag20", firstTag.TagName);
        Assert.Equal(200m, firstTag.GrossAmount);
    }

    [Fact]
    public async Task Handle_TransactionsWithSameTagId_ConsolidatesCorrectly()
    {
        // Arrange - Multiple transactions with identical tag (same ID and name)
        var tag = CreateTag(1, "Recurring");

        var transactions = new[]
        {
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-10), TransactionType.Debit, [tag]),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-8), TransactionType.Debit, [tag]),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-6), TransactionType.Debit, [tag]),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-4), TransactionType.Debit, [tag]),
            CreateTransaction(TestAccountId, -100m, DateTime.Today.AddDays(-2), TransactionType.Debit, [tag]),
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
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert - Should have one entry for Recurring with total 500
        var recurringTags = result.Tags.Where(t => t.TagName == "Recurring").ToList();
        Assert.Single(recurringTags);
        Assert.Equal(500m, recurringTags[0].GrossAmount);
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

    private static DomainTransaction CreateTransactionWithMultipleSplits(
        Guid accountId,
        decimal totalAmount,
        DateTime transactionTime,
        TransactionType transactionType,
        IEnumerable<(decimal Amount, IEnumerable<DomainTag> Tags)> splits)
    {
        var transactionId = Guid.NewGuid();
        var transaction = new DomainTransaction(transactionId)
        {
            AccountId = accountId,
            Amount = totalAmount,
            TransactionTime = transactionTime,
            TransactionType = transactionType,
            Source = "Test",
            ExcludeFromReporting = false,
        };

        var splitsField = typeof(DomainTransaction).GetField("_splits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var splitsList = (List<DomainTransactionSplit>)splitsField!.GetValue(transaction)!;

        foreach (var (amount, tags) in splits)
        {
            var split = new DomainTransactionSplit(Guid.NewGuid())
            {
                TransactionId = transactionId,
                Amount = amount,
                Tags = [.. tags],
            };
            splitsList.Add(split);
        }

        return transaction;
    }

    #endregion
}
