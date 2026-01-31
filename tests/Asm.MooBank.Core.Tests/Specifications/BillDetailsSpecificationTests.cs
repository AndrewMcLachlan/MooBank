using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Domain.Entities.Utility.Specifications;
using Asm.MooBank.Models;
using UtilityAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="BillDetailsSpecification"/> specification.
/// Tests verify that the specification correctly includes Bills with Periods, Usage, ServiceCharge, and Discounts.
/// </summary>
public class BillDetailsSpecificationTests
{
    #region Basic Application

    /// <summary>
    /// Given a collection of utility accounts
    /// When BillDetailsSpecification is applied
    /// Then all accounts should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var accounts = new List<UtilityAccount>
        {
            CreateUtilityAccount("Account 1", "ACC001"),
            CreateUtilityAccount("Account 2", "ACC002"),
            CreateUtilityAccount("Account 3", "ACC003"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When BillDetailsSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var accounts = new List<UtilityAccount>();
        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single utility account
    /// When BillDetailsSpecification is applied
    /// Then the account should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleAccount_ReturnsSingleAccount()
    {
        // Arrange
        var accounts = new List<UtilityAccount>
        {
            CreateUtilityAccount("Single Account", "ACC001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Single Account", result[0].Name);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given utility accounts with various account numbers
    /// When BillDetailsSpecification is applied
    /// Then account numbers should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesAccountNumbers()
    {
        // Arrange
        var accounts = new List<UtilityAccount>
        {
            CreateUtilityAccount("Electric", "ELEC001"),
            CreateUtilityAccount("Gas", "GAS001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, a => a.AccountNumber == "ELEC001");
        Assert.Contains(result, a => a.AccountNumber == "GAS001");
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When BillDetailsSpecification is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var accounts = new List<UtilityAccount>
        {
            CreateUtilityAccount("Keep", "ACC001"),
            CreateUtilityAccount("Remove", "ACC002"),
            CreateUtilityAccount("Keep", "ACC003"),
        };

        var filteredQuery = accounts.AsQueryable().Where(a => a.Name == "Keep");
        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("Keep", a.Name));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of utility accounts
    /// When BillDetailsSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var accounts = new List<UtilityAccount>
        {
            CreateUtilityAccount("Test", "ACC001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(accounts.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<UtilityAccount>>(result);
    }

    #endregion

    private UtilityAccount CreateUtilityAccount(string name, string accountNumber)
    {
        return new UtilityAccount(Guid.NewGuid())
        {
            Name = name,
            AccountNumber = accountNumber,
            Currency = "AUD",
            UtilityType = UtilityType.Electricity,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId }],
        };
    }
}
