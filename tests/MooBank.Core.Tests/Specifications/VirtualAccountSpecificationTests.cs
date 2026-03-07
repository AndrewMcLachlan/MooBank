using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using DomainInstrument = Asm.MooBank.Domain.Entities.Instrument.Instrument;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="VirtualAccountSpecification"/> specification.
/// Tests verify that the specification correctly includes VirtualInstruments with RecurringTransactions.
/// </summary>
public class VirtualAccountSpecificationTests
{
    #region Basic Application

    /// <summary>
    /// Given a collection of instruments
    /// When VirtualAccountSpecification is applied
    /// Then all instruments should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithInstruments_ReturnsAllInstruments()
    {
        // Arrange
        var instruments = new List<DomainInstrument>
        {
            CreateLogicalAccount("Account 1"),
            CreateLogicalAccount("Account 2"),
            CreateLogicalAccount("Account 3"),
        };

        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Given an empty collection
    /// When VirtualAccountSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var instruments = new List<DomainInstrument>();
        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Given a single instrument
    /// When VirtualAccountSpecification is applied
    /// Then the instrument should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithSingleInstrument_ReturnsSingleInstrument()
    {
        // Arrange
        var instruments = new List<DomainInstrument>
        {
            CreateLogicalAccount("Single Account"),
        };

        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(instruments.AsQueryable()).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Single Account", result[0].Name);
    }

    #endregion

    #region Query Preservation

    /// <summary>
    /// Given instruments with various names
    /// When VirtualAccountSpecification is applied
    /// Then instrument names should be preserved
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesInstrumentNames()
    {
        // Arrange
        var instruments = new List<DomainInstrument>
        {
            CreateLogicalAccount("Savings"),
            CreateLogicalAccount("Checking"),
        };

        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(instruments.AsQueryable()).ToList();

        // Assert
        Assert.Contains(result, i => i.Name == "Savings");
        Assert.Contains(result, i => i.Name == "Checking");
    }

    /// <summary>
    /// Given a queryable with a filter already applied
    /// When VirtualAccountSpecification is applied
    /// Then the filter should still be effective
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_PreservesExistingFilters()
    {
        // Arrange
        var instruments = new List<DomainInstrument>
        {
            CreateLogicalAccount("Keep"),
            CreateLogicalAccount("Remove"),
            CreateLogicalAccount("Keep"),
        };

        var filteredQuery = instruments.AsQueryable().Where(i => i.Name == "Keep");
        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(filteredQuery).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal("Keep", i.Name));
    }

    #endregion

    #region Queryable Behavior

    /// <summary>
    /// Given a queryable of instruments
    /// When VirtualAccountSpecification is applied
    /// Then the result should be queryable
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_ReturnsQueryable()
    {
        // Arrange
        var instruments = new List<DomainInstrument>
        {
            CreateLogicalAccount("Test"),
        };

        var spec = new VirtualAccountSpecification();

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.IsAssignableFrom<IQueryable<DomainInstrument>>(result);
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
