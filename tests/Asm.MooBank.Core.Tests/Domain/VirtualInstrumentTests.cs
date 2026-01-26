using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="VirtualInstrument"/> domain entity.
/// Tests cover recurring transaction management.
/// </summary>
public class VirtualInstrumentTests
{
    private readonly TestEntities _entities = new();

    #region AddRecurringTransaction

    /// <summary>
    /// Given a VirtualInstrument with no recurring transactions
    /// When AddRecurringTransaction is called
    /// Then RecurringTransactions.Count should be 1
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_ToEmptyCollection_AddsTransaction()
    {
        // Arrange
        var virtualInstrument = _entities.CreateVirtualInstrument();

        // Act
        virtualInstrument.AddRecurringTransaction("Monthly Savings", 500m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Single(virtualInstrument.RecurringTransactions);
    }

    /// <summary>
    /// Given a VirtualInstrument
    /// When AddRecurringTransaction is called
    /// Then the returned RecurringTransaction should have correct properties
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_SetsCorrectProperties()
    {
        // Arrange
        var virtualInstrument = _entities.CreateVirtualInstrument();
        var nextRun = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        // Act
        var result = virtualInstrument.AddRecurringTransaction("Weekly Transfer", 100m, ScheduleFrequency.Weekly, nextRun);

        // Assert
        Assert.Equal("Weekly Transfer", result.Description);
        Assert.Equal(100m, result.Amount);
        Assert.Equal(ScheduleFrequency.Weekly, result.Schedule);
        Assert.Equal(nextRun, result.NextRun);
        Assert.Equal(virtualInstrument.Id, result.VirtualAccountId);
    }

    /// <summary>
    /// Given a VirtualInstrument with existing recurring transactions
    /// When AddRecurringTransaction is called multiple times
    /// Then all transactions should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_MultipleTimes_AddsAll()
    {
        // Arrange
        var virtualInstrument = _entities.CreateVirtualInstrument();
        var nextRun = DateOnly.FromDateTime(DateTime.Today);

        // Act
        virtualInstrument.AddRecurringTransaction("Weekly", 100m, ScheduleFrequency.Weekly, nextRun);
        virtualInstrument.AddRecurringTransaction("Monthly", 500m, ScheduleFrequency.Monthly, nextRun);
        virtualInstrument.AddRecurringTransaction("Yearly", 1000m, ScheduleFrequency.Yearly, nextRun);

        // Assert
        Assert.Equal(3, virtualInstrument.RecurringTransactions.Count);
    }

    /// <summary>
    /// Given a VirtualInstrument
    /// When AddRecurringTransaction is called with null description
    /// Then the transaction should be created with null description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_WithNullDescription_CreatesTransaction()
    {
        // Arrange
        var virtualInstrument = _entities.CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction(null, 50m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Null(result.Description);
        Assert.Single(virtualInstrument.RecurringTransactions);
    }

    #endregion
}
