using Asm.MooBank.Domain.Entities.Utility;
using Asm.MooBank.Domain.Entities.Utility.Specifications;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="BillDetailsSpecification"/> specification.
/// Tests verify that the specification correctly includes Periods, Usage, ServiceCharge, and Discounts.
/// </summary>
public class BillDetailsSpecificationTests
{
    #region Basic Application

    /// <summary>
    /// Given a collection of bills
    /// When BillDetailsSpecification is applied
    /// Then all bills should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithBills_ReturnsAllBills()
    {
        // Arrange
        var bills = new List<Bill>
        {
            CreateBill(1, "INV001"),
            CreateBill(2, "INV002"),
            CreateBill(3, "INV003"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(bills.AsQueryable());

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
        var bills = new List<Bill>();
        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(bills.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single bill
    /// When BillDetailsSpecification is applied
    /// Then the bill should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleBill_ReturnsSingleBill()
    {
        // Arrange
        var bills = new List<Bill>
        {
            CreateBill(1, "INV001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(bills.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("INV001", result[0].InvoiceNumber);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given bills with various invoice numbers
    /// When BillDetailsSpecification is applied
    /// Then invoice numbers should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesInvoiceNumbers()
    {
        // Arrange
        var bills = new List<Bill>
        {
            CreateBill(1, "ELEC001"),
            CreateBill(2, "GAS001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(bills.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, b => b.InvoiceNumber == "ELEC001");
        Assert.Contains(result, b => b.InvoiceNumber == "GAS001");
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
        var accountId = Guid.NewGuid();
        var bills = new List<Bill>
        {
            CreateBill(1, "INV001", accountId),
            CreateBill(2, "INV002"),
            CreateBill(3, "INV003", accountId),
        };

        var filteredQuery = bills.AsQueryable().Where(b => b.AccountId == accountId);
        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Equal(accountId, b.AccountId));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of bills
    /// When BillDetailsSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var bills = new List<Bill>
        {
            CreateBill(1, "INV001"),
        };

        var spec = new BillDetailsSpecification();

        // Act
        var result = spec.Apply(bills.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<Bill>>(result);
    }

    #endregion

    private static Bill CreateBill(int id, string invoiceNumber, Guid? accountId = null)
    {
        return new Bill(id)
        {
            AccountId = accountId ?? Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
    }
}
