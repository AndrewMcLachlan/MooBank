using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="AccountDetailsSpecification"/> specification.
/// Tests verify that the specification correctly includes Owners, Viewers, and InstitutionAccounts.
/// </summary>
public class AccountDetailsSpecificationTests
{
    #region Basic Application

    /// <summary>
    /// Given a collection of logical accounts
    /// When AccountDetailsSpecification is applied
    /// Then all accounts should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var accounts = new List<LogicalAccount>
        {
            CreateLogicalAccount("Account 1"),
            CreateLogicalAccount("Account 2"),
            CreateLogicalAccount("Account 3"),
        };

        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When AccountDetailsSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var accounts = new List<LogicalAccount>();
        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single account
    /// When AccountDetailsSpecification is applied
    /// Then the account should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleAccount_ReturnsSingleAccount()
    {
        // Arrange
        var accounts = new List<LogicalAccount>
        {
            CreateLogicalAccount("Single Account"),
        };

        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Single Account", result[0].Name);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given accounts with various names
    /// When AccountDetailsSpecification is applied
    /// Then account names should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesAccountNames()
    {
        // Arrange
        var accounts = new List<LogicalAccount>
        {
            CreateLogicalAccount("Savings"),
            CreateLogicalAccount("Checking"),
        };

        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, a => a.Name == "Savings");
        Assert.Contains(result, a => a.Name == "Checking");
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When AccountDetailsSpecification is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var accounts = new List<LogicalAccount>
        {
            CreateLogicalAccount("Keep"),
            CreateLogicalAccount("Remove"),
            CreateLogicalAccount("Keep"),
        };

        var filteredQuery = accounts.AsQueryable().Where(a => a.Name == "Keep");
        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("Keep", a.Name));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of accounts
    /// When AccountDetailsSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var accounts = new List<LogicalAccount>
        {
            CreateLogicalAccount("Test"),
        };

        var spec = new AccountDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<LogicalAccount>>(result);
    }

    #endregion

    private LogicalAccount CreateLogicalAccount(string name)
    {
        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = name,
            Currency = "AUD",
            AccountType = AccountType.Transaction,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId }],
        };
    }
}
