using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="SortSpecification"/> specification.
/// Tests verify transaction sorting logic by various fields and directions.
/// </summary>
public class SortSpecificationTests
{
    #region Default Sorting

    /// <summary>
    /// Given transactions with various dates
    /// When SortSpecification is applied with no field specified
    /// Then transactions should be sorted by TransactionTime ascending
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNoFieldAscending_SortsByTransactionTimeAscending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Middle", now.AddDays(-5)),
            CreateTransaction(-50m, "Oldest", now.AddDays(-10)),
            CreateTransaction(-25m, "Newest", now),
        };

        var sortable = new TestSortable(null, SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Oldest", result[0].Description);
        Assert.Equal("Middle", result[1].Description);
        Assert.Equal("Newest", result[2].Description);
    }

    /// <summary>
    /// Given transactions with various dates
    /// When SortSpecification is applied with no field specified and descending direction
    /// Then transactions should be sorted by TransactionTime descending
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNoFieldDescending_SortsByTransactionTimeDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Middle", now.AddDays(-5)),
            CreateTransaction(-50m, "Oldest", now.AddDays(-10)),
            CreateTransaction(-25m, "Newest", now),
        };

        var sortable = new TestSortable(null, SortDirection.Descending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Newest", result[0].Description);
        Assert.Equal("Middle", result[1].Description);
        Assert.Equal("Oldest", result[2].Description);
    }

    #endregion

    #region Description Sorting

    /// <summary>
    /// Given transactions with various descriptions
    /// When SortSpecification is applied with Description field
    /// Then transactions should be sorted alphabetically by description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDescriptionFieldAscending_SortsByDescriptionAscending()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Walmart"),
            CreateTransaction(-50m, "Amazon"),
            CreateTransaction(-25m, "Costco"),
        };

        var sortable = new TestSortable("Description", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Amazon", result[0].Description);
        Assert.Equal("Costco", result[1].Description);
        Assert.Equal("Walmart", result[2].Description);
    }

    /// <summary>
    /// Given transactions with various descriptions
    /// When SortSpecification is applied with Description field descending
    /// Then transactions should be sorted reverse alphabetically by description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDescriptionFieldDescending_SortsByDescriptionDescending()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Walmart"),
            CreateTransaction(-50m, "Amazon"),
            CreateTransaction(-25m, "Costco"),
        };

        var sortable = new TestSortable("Description", SortDirection.Descending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Walmart", result[0].Description);
        Assert.Equal("Costco", result[1].Description);
        Assert.Equal("Amazon", result[2].Description);
    }

    #endregion

    #region Amount Sorting

    /// <summary>
    /// Given transactions with various amounts (including negative)
    /// When SortSpecification is applied with Amount field
    /// Then transactions should be sorted by absolute amount value
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAmountFieldAscending_SortsByAbsoluteAmountAscending()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-100m, "Large Debit"),
            CreateTransaction(-25m, "Small Debit"),
            CreateTransaction(-50m, "Medium Debit"),
        };

        var sortable = new TestSortable("Amount", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Small Debit", result[0].Description);
        Assert.Equal("Medium Debit", result[1].Description);
        Assert.Equal("Large Debit", result[2].Description);
    }

    /// <summary>
    /// Given transactions with various amounts (including negative)
    /// When SortSpecification is applied with Amount field descending
    /// Then transactions should be sorted by absolute amount value descending
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAmountFieldDescending_SortsByAbsoluteAmountDescending()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-100m, "Large Debit"),
            CreateTransaction(-25m, "Small Debit"),
            CreateTransaction(-50m, "Medium Debit"),
        };

        var sortable = new TestSortable("Amount", SortDirection.Descending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Large Debit", result[0].Description);
        Assert.Equal("Medium Debit", result[1].Description);
        Assert.Equal("Small Debit", result[2].Description);
    }

    /// <summary>
    /// Given both debit and credit transactions
    /// When SortSpecification is applied with Amount field
    /// Then transactions should be sorted by absolute amount (debits and credits treated equally)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAmountField_TreatsDebitsAndCreditsEqually()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-75m, "Large Debit"),   // abs = 75
            CreateTransaction(100m, "Large Credit"),  // abs = 100
            CreateTransaction(-25m, "Small Debit"),   // abs = 25
            CreateTransaction(50m, "Medium Credit"),  // abs = 50
        };

        var sortable = new TestSortable("Amount", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Small Debit", result[0].Description);   // 25
        Assert.Equal("Medium Credit", result[1].Description); // 50
        Assert.Equal("Large Debit", result[2].Description);   // 75
        Assert.Equal("Large Credit", result[3].Description);  // 100
    }

    #endregion

    #region TransactionTime Sorting

    /// <summary>
    /// Given transactions with various dates
    /// When SortSpecification is applied with TransactionTime field
    /// Then transactions should be sorted by date
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithTransactionTimeFieldAscending_SortsByDateAscending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Middle", now.AddDays(-5)),
            CreateTransaction(-50m, "Oldest", now.AddDays(-10)),
            CreateTransaction(-25m, "Newest", now),
        };

        var sortable = new TestSortable("TransactionTime", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal("Oldest", result[0].Description);
        Assert.Equal("Middle", result[1].Description);
        Assert.Equal("Newest", result[2].Description);
    }

    #endregion

    #region Empty Field Handling

    /// <summary>
    /// Given transactions
    /// When SortSpecification is applied with empty string field
    /// Then default sorting (TransactionTime) should be used
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyField_UsesDefaultSorting()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Middle", now.AddDays(-5)),
            CreateTransaction(-50m, "Oldest", now.AddDays(-10)),
            CreateTransaction(-25m, "Newest", now),
        };

        var sortable = new TestSortable("", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert - Should default to TransactionTime ascending
        Assert.Equal("Oldest", result[0].Description);
        Assert.Equal("Middle", result[1].Description);
        Assert.Equal("Newest", result[2].Description);
    }

    /// <summary>
    /// Given transactions
    /// When SortSpecification is applied with whitespace-only field
    /// Then default sorting (TransactionTime) should be used
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithWhitespaceField_UsesDefaultSorting()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-30m, "Middle", now.AddDays(-5)),
            CreateTransaction(-50m, "Oldest", now.AddDays(-10)),
            CreateTransaction(-25m, "Newest", now),
        };

        var sortable = new TestSortable("   ", SortDirection.Descending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert - Should default to TransactionTime descending
        Assert.Equal("Newest", result[0].Description);
        Assert.Equal("Middle", result[1].Description);
        Assert.Equal("Oldest", result[2].Description);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given an empty collection of transactions
    /// When SortSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var transactions = new List<DomainTransaction>();
        var sortable = new TestSortable("TransactionTime", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single transaction
    /// When SortSpecification is applied
    /// Then the single transaction should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleTransaction_ReturnsSingleTransaction()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-50m, "Only One"),
        };
        var sortable = new TestSortable("Amount", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
    }

    /// <summary>
    /// Given transactions with same description
    /// When SortSpecification is applied with Description field
    /// Then order should be stable (maintain original order for equal elements)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSameDescriptions_MaintainsStableOrder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-50m, "Same", now),
            CreateTransaction(-30m, "Same", now.AddMinutes(1)),
            CreateTransaction(-25m, "Same", now.AddMinutes(2)),
        };

        var sortable = new TestSortable("Description", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, t => Assert.Equal("Same", t.Description));
    }

    /// <summary>
    /// Given transactions with same absolute amount but different signs
    /// When SortSpecification is applied with Amount field
    /// Then both should appear adjacent in sorted results
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSameAbsoluteAmount_GroupsTogether()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-100m, "Debit 100"),
            CreateTransaction(100m, "Credit 100"),
            CreateTransaction(-50m, "Debit 50"),
            CreateTransaction(50m, "Credit 50"),
        };

        var sortable = new TestSortable("Amount", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        // 50s should come before 100s
        var first50Index = result.FindIndex(t => Math.Abs(t.Amount) == 50m);
        var first100Index = result.FindIndex(t => Math.Abs(t.Amount) == 100m);
        Assert.True(first50Index < first100Index);
    }

    /// <summary>
    /// Given transactions with same time
    /// When SortSpecification is applied with TransactionTime field
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSameTransactionTime_ReturnsAllTransactions()
    {
        // Arrange
        var sameTime = DateTime.UtcNow;
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-50m, "First", sameTime),
            CreateTransaction(-30m, "Second", sameTime),
            CreateTransaction(-25m, "Third", sameTime),
        };

        var sortable = new TestSortable("TransactionTime", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Given transactions with null descriptions
    /// When SortSpecification is applied with Description field
    /// Then null descriptions should be handled correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNullDescriptions_HandlesNullsCorrectly()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            CreateTransaction(-50m, "Zebra"),
            CreateTransactionWithNullDescription(-30m),
            CreateTransaction(-25m, "Apple"),
        };

        var sortable = new TestSortable("Description", SortDirection.Ascending);
        var spec = new SortSpecification(sortable);

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert - Should not throw, null descriptions handled
        Assert.Equal(3, result.Count);
    }

    #endregion

    private DomainTransaction CreateTransaction(decimal amount, string description, DateTime? transactionTime = null)
    {
        return DomainTransaction.Create(
            TestModels.AccountId,
            TestModels.UserId,
            amount,
            description,
            transactionTime ?? DateTime.UtcNow,
            null,
            "Test",
            null);
    }

    private DomainTransaction CreateTransactionWithNullDescription(decimal amount, DateTime? transactionTime = null)
    {
        return DomainTransaction.Create(
            TestModels.AccountId,
            TestModels.UserId,
            amount,
            null,
            transactionTime ?? DateTime.UtcNow,
            null,
            "Test",
            null);
    }

    private record TestSortable(string? SortField, SortDirection SortDirection) : ISortable;
}
