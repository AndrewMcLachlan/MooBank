using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="FilterSpecification"/> specification.
/// Tests verify transaction filtering logic for description, type, date range, tags, and more.
/// </summary>
public class FilterSpecificationTests
{
    private readonly TestEntities _entities = new();

    #region InstrumentId Filter

    /// <summary>
    /// Given transactions from multiple accounts
    /// When FilterSpecification is applied with an InstrumentId
    /// Then only transactions from that account should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithInstrumentId_ReturnsOnlyMatchingAccount()
    {
        // Arrange
        var targetAccountId = TestModels.AccountId;
        var otherAccountId = Guid.NewGuid();

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(targetAccountId, -50m, "Transaction 1"),
            CreateTransaction(targetAccountId, -30m, "Transaction 2"),
            CreateTransaction(otherAccountId, -100m, "Other Account"),
        };

        var filter = new TransactionFilter { InstrumentId = targetAccountId };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(targetAccountId, t.AccountId));
    }

    #endregion

    #region Description Filter

    /// <summary>
    /// Given transactions with various descriptions
    /// When FilterSpecification is applied with a single filter term
    /// Then only transactions with matching descriptions should be returned
    /// Note: Uses EF.Functions.Like which requires EF Core database context
    /// </summary>
    [Fact(Skip = "Requires EF Core - EF.Functions.Like not supported in LINQ to Objects")]
    [Trait("Category", "Integration")]
    public void Apply_WithDescriptionFilter_ReturnsMatchingDescriptions()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Costco Purchase"),
            CreateTransaction(TestModels.AccountId, -30m, "Walmart Shopping"),
            CreateTransaction(TestModels.AccountId, -25m, "Costco Gas"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Filter = "Costco",
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Contains("Costco", t.Description));
    }

    /// <summary>
    /// Given transactions with various descriptions
    /// When FilterSpecification is applied with comma-separated filter terms
    /// Then transactions matching any term should be returned
    /// Note: Uses EF.Functions.Like which requires EF Core database context
    /// </summary>
    [Fact(Skip = "Requires EF Core - EF.Functions.Like not supported in LINQ to Objects")]
    [Trait("Category", "Integration")]
    public void Apply_WithCommaSeparatedFilters_ReturnsMatchingAnyTerm()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Costco Purchase"),
            CreateTransaction(TestModels.AccountId, -30m, "Walmart Shopping"),
            CreateTransaction(TestModels.AccountId, -25m, "Target Groceries"),
            CreateTransaction(TestModels.AccountId, -15m, "Amazon Order"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Filter = "Costco,Target",
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given transactions
    /// When FilterSpecification is applied with null filter
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNullFilter_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Transaction 1"),
            CreateTransaction(TestModels.AccountId, -30m, "Transaction 2"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Filter = null,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Transaction Type Filter

    /// <summary>
    /// Given both debit and credit transactions
    /// When FilterSpecification is applied with Debit type
    /// Then only debit transactions (negative amounts) should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDebitFilter_ReturnsOnlyDebits()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Debit 1"),
            CreateTransaction(TestModels.AccountId, 100m, "Credit 1"),
            CreateTransaction(TestModels.AccountId, -30m, "Debit 2"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TransactionType = TransactionFilterType.Debit,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Amount < 0));
    }

    /// <summary>
    /// Given both debit and credit transactions
    /// When FilterSpecification is applied with Credit type
    /// Then only credit transactions (positive amounts) should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithCreditFilter_ReturnsOnlyCredits()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Debit 1"),
            CreateTransaction(TestModels.AccountId, 100m, "Credit 1"),
            CreateTransaction(TestModels.AccountId, 75m, "Credit 2"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TransactionType = TransactionFilterType.Credit,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Amount > 0));
    }

    /// <summary>
    /// Given both debit and credit transactions
    /// When FilterSpecification is applied with None type
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNoneTypeFilter_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Debit 1"),
            CreateTransaction(TestModels.AccountId, 100m, "Credit 1"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TransactionType = TransactionFilterType.None,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Date Range Filter

    /// <summary>
    /// Given transactions with various dates
    /// When FilterSpecification is applied with start date
    /// Then only transactions on or after start date should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithStartDate_ReturnsTransactionsAfterStart()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Old", now.AddDays(-10)),
            CreateTransaction(TestModels.AccountId, -30m, "Recent", now.AddDays(-2)),
            CreateTransaction(TestModels.AccountId, -25m, "Today", now),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Start = now.AddDays(-5),
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given transactions with various dates
    /// When FilterSpecification is applied with end date
    /// Then only transactions on or before end date should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEndDate_ReturnsTransactionsBeforeEnd()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Old", now.AddDays(-10)),
            CreateTransaction(TestModels.AccountId, -30m, "Mid", now.AddDays(-5)),
            CreateTransaction(TestModels.AccountId, -25m, "Recent", now),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            End = now.AddDays(-4),
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given transactions with various dates
    /// When FilterSpecification is applied with both start and end dates
    /// Then only transactions within the range should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDateRange_ReturnsTransactionsInRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "Too Old", now.AddDays(-15)),
            CreateTransaction(TestModels.AccountId, -30m, "In Range 1", now.AddDays(-7)),
            CreateTransaction(TestModels.AccountId, -25m, "In Range 2", now.AddDays(-3)),
            CreateTransaction(TestModels.AccountId, -20m, "Too Recent", now),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Start = now.AddDays(-10),
            End = now.AddDays(-1),
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Untagged Only Filter

    /// <summary>
    /// Given tagged and untagged transactions
    /// When FilterSpecification is applied with UntaggedOnly
    /// Then only untagged transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithUntaggedOnly_ReturnsOnlyUntaggedTransactions()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Groceries");
        var taggedTransaction = CreateTransaction(TestModels.AccountId, -50m, "Tagged");
        taggedTransaction.Splits.First().Tags.Add(tag);

        var untaggedTransaction = CreateTransaction(TestModels.AccountId, -30m, "Untagged");

        var transactions = new List<DomainTransaction> { taggedTransaction, untaggedTransaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Untagged = "untagged",
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Untagged", result.First().Description);
    }

    /// <summary>
    /// Given tagged and untagged transactions
    /// When FilterSpecification is applied without UntaggedOnly
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithoutUntaggedOnly_ReturnsAllTransactions()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Groceries");
        var taggedTransaction = CreateTransaction(TestModels.AccountId, -50m, "Tagged");
        taggedTransaction.Splits.First().Tags.Add(tag);

        var untaggedTransaction = CreateTransaction(TestModels.AccountId, -30m, "Untagged");

        var transactions = new List<DomainTransaction> { taggedTransaction, untaggedTransaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Tag IDs Filter

    /// <summary>
    /// Given transactions with various tags
    /// When FilterSpecification is applied with specific TagIds
    /// Then only transactions with those tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithTagIds_ReturnsOnlyMatchingTags()
    {
        // Arrange
        var groceriesTag = _entities.CreateTag(1, "Groceries");
        var entertainmentTag = _entities.CreateTag(2, "Entertainment");
        var utilitiesTag = _entities.CreateTag(3, "Utilities");

        var groceryTransaction = CreateTransaction(TestModels.AccountId, -50m, "Grocery");
        groceryTransaction.Splits.First().Tags.Add(groceriesTag);

        var entertainmentTransaction = CreateTransaction(TestModels.AccountId, -30m, "Entertainment");
        entertainmentTransaction.Splits.First().Tags.Add(entertainmentTag);

        var utilitiesTransaction = CreateTransaction(TestModels.AccountId, -100m, "Utilities");
        utilitiesTransaction.Splits.First().Tags.Add(utilitiesTag);

        var transactions = new List<DomainTransaction>
        {
            groceryTransaction,
            entertainmentTransaction,
            utilitiesTransaction,
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TagIds = [1, 2], // Groceries and Entertainment
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, t => t.Description == "Utilities");
    }

    /// <summary>
    /// Given transactions with various tags
    /// When FilterSpecification is applied with empty TagIds
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyTagIds_ReturnsAllTransactions()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Tag");
        var taggedTransaction = CreateTransaction(TestModels.AccountId, -50m, "Tagged");
        taggedTransaction.Splits.First().Tags.Add(tag);

        var untaggedTransaction = CreateTransaction(TestModels.AccountId, -30m, "Untagged");

        var transactions = new List<DomainTransaction> { taggedTransaction, untaggedTransaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TagIds = [],
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region Combined Filters

    /// <summary>
    /// Given various transactions
    /// When FilterSpecification is applied with multiple filters (type, date, tags)
    /// Then only transactions matching all criteria should be returned
    /// Note: Does not test description filter as it requires EF Core
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithCombinedFilters_ReturnsMatchingAllCriteria()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var tag = _entities.CreateTag(1, "Groceries");

        // Matches: debit, has tag, in date range
        var matchingTransaction = CreateTransaction(TestModels.AccountId, -50m, "Matching", now.AddDays(-3));
        matchingTransaction.Splits.First().Tags.Add(tag);

        // Doesn't match: credit
        var creditTransaction = CreateTransaction(TestModels.AccountId, 100m, "Credit", now.AddDays(-3));
        creditTransaction.Splits.First().Tags.Add(tag);

        // Doesn't match: no tag
        var noTagTransaction = CreateTransaction(TestModels.AccountId, -30m, "No Tag", now.AddDays(-3));

        // Doesn't match: outside date range
        var oldTransaction = CreateTransaction(TestModels.AccountId, -40m, "Old", now.AddDays(-20));
        oldTransaction.Splits.First().Tags.Add(tag);

        var transactions = new List<DomainTransaction>
        {
            matchingTransaction,
            creditTransaction,
            noTagTransaction,
            oldTransaction,
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TransactionType = TransactionFilterType.Debit,
            Start = now.AddDays(-10),
            End = now,
            TagIds = [1],
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Matching", result.First().Description);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given an empty collection of transactions
    /// When FilterSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var transactions = new List<DomainTransaction>();
        var filter = new TransactionFilter { InstrumentId = TestModels.AccountId };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given transactions with null TagIds in filter
    /// When FilterSpecification is applied
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNullTagIds_ReturnsAllTransactions()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Tag");
        var taggedTransaction = CreateTransaction(TestModels.AccountId, -50m, "Tagged");
        taggedTransaction.Splits.First().Tags.Add(tag);

        var untaggedTransaction = CreateTransaction(TestModels.AccountId, -30m, "Untagged");

        var transactions = new List<DomainTransaction> { taggedTransaction, untaggedTransaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TagIds = null,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given transactions with the same tag
    /// When FilterSpecification is applied with that TagId
    /// Then all transactions with that tag should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithTagIdMatchingMultiple_ReturnsAllMatching()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "SharedTag");

        var transaction1 = CreateTransaction(TestModels.AccountId, -50m, "Transaction 1");
        transaction1.Splits.First().Tags.Add(tag);

        var transaction2 = CreateTransaction(TestModels.AccountId, -30m, "Transaction 2");
        transaction2.Splits.First().Tags.Add(tag);

        var transaction3 = CreateTransaction(TestModels.AccountId, -25m, "No Tag");

        var transactions = new List<DomainTransaction> { transaction1, transaction2, transaction3 };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TagIds = [1],
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given transactions with multiple tags on a single split
    /// When FilterSpecification is applied with one of those TagIds
    /// Then the transaction should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithMultipleTagsOnSplit_MatchesAnyTag()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Food");
        var tag3 = _entities.CreateTag(3, "Utilities");

        var transaction = CreateTransaction(TestModels.AccountId, -50m, "Multi-tagged");
        transaction.Splits.First().Tags.Add(tag1);
        transaction.Splits.First().Tags.Add(tag2);

        var transactions = new List<DomainTransaction> { transaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TagIds = [2], // Only searching for tag2
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Single(result);
    }

    /// <summary>
    /// Given transactions with zero amount
    /// When FilterSpecification is applied with Credit type
    /// Then zero amount transactions should be excluded (0 is not > 0)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithZeroAmountAndCreditFilter_ExcludesZeroAmount()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, 100m, "Credit"),
            CreateTransaction(TestModels.AccountId, 0m, "Zero"),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            TransactionType = TransactionFilterType.Credit,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Credit", result.First().Description);
    }

    /// <summary>
    /// Given transactions at exact boundary of date range
    /// When FilterSpecification is applied
    /// Then boundary transactions should be included
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDateAtExactBoundary_IncludesBoundary()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(TestModels.AccountId, -50m, "At Start", startDate),
            CreateTransaction(TestModels.AccountId, -30m, "At End", endDate),
            CreateTransaction(TestModels.AccountId, -25m, "Before Start", startDate.AddSeconds(-1)),
            CreateTransaction(TestModels.AccountId, -20m, "After End", endDate.AddSeconds(1)),
        };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Start = startDate,
            End = endDate,
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Description == "At Start");
        Assert.Contains(result, t => t.Description == "At End");
    }

    /// <summary>
    /// Given transactions with UntaggedOnly false (not "untagged" string)
    /// When FilterSpecification is applied
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithUntaggedOnlyFalse_ReturnsAllTransactions()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Tag");
        var taggedTransaction = CreateTransaction(TestModels.AccountId, -50m, "Tagged");
        taggedTransaction.Splits.First().Tags.Add(tag);

        var untaggedTransaction = CreateTransaction(TestModels.AccountId, -30m, "Untagged");

        var transactions = new List<DomainTransaction> { taggedTransaction, untaggedTransaction };

        var filter = new TransactionFilter
        {
            InstrumentId = TestModels.AccountId,
            Untagged = null, // Not filtering by untagged
        };
        var spec = new FilterSpecification(filter);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    private DomainTransaction CreateTransaction(Guid accountId, decimal amount, string description, DateTime? transactionTime = null)
    {
        return DomainTransaction.Create(
            accountId,
            TestModels.UserId,
            amount,
            description,
            transactionTime ?? DateTime.UtcNow,
            null,
            "Test",
            null);
    }
}
