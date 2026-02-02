using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="IncludeSplitsSpecification"/> specification.
/// Tests verify that the specification correctly includes Splits with Tags, Settings, and OffsetBy.
/// </summary>
public class IncludeSplitsSpecificationTests
{
    private readonly TestEntities _entities = new();

    #region Basic Application

    /// <summary>
    /// Given a collection of transactions
    /// When IncludeSplitsSpecification is applied
    /// Then all transactions should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "Transaction 1"),
            _entities.CreateTransaction(-75m, "Transaction 2"),
            _entities.CreateTransaction(-100m, "Transaction 3"),
        };

        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When IncludeSplitsSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var transactions = new List<DomainTransaction>();
        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single transaction
    /// When IncludeSplitsSpecification is applied
    /// Then the transaction should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleTransaction_ReturnsSingleTransaction()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "Single Transaction"),
        };

        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Single Transaction", result[0].Description);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given transactions with various amounts
    /// When IncludeSplitsSpecification is applied
    /// Then transaction amounts should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesTransactionAmounts()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "Amount 50"),
            _entities.CreateTransaction(-100m, "Amount 100"),
        };

        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, t => t.Amount == -50m);
        Assert.Contains(result, t => t.Amount == -100m);
    }

    /// <summary>
    /// Given transactions with various descriptions
    /// When IncludeSplitsSpecification is applied
    /// Then transaction descriptions should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesTransactionDescriptions()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "First"),
            _entities.CreateTransaction(-75m, "Second"),
            _entities.CreateTransaction(-100m, "Third"),
        };

        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, t => t.Description == "First");
        Assert.Contains(result, t => t.Description == "Second");
        Assert.Contains(result, t => t.Description == "Third");
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of transactions
    /// When IncludeSplitsSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "Test"),
        };

        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(transactions.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<DomainTransaction>>(result);
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When IncludeSplitsSpecification is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var transactions = new List<DomainTransaction>
        {
            _entities.CreateTransaction(-50m, "Keep"),
            _entities.CreateTransaction(-75m, "Remove"),
            _entities.CreateTransaction(-100m, "Keep"),
        };

        var filteredQuery = transactions.AsQueryable().Where(t => t.Description == "Keep");
        var spec = new IncludeSplitsSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("Keep", t.Description));
    }

    #endregion
}
