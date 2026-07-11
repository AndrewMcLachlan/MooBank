using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Models;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="VirtualInstrument"/> entity.
/// Tests verify recurring transaction management.
/// </summary>
public class VirtualInstrumentTests
{
    #region AddRecurringTransaction

    /// <summary>
    /// Given a virtual instrument with no recurring transactions
    /// When AddRecurringTransaction is called
    /// Then a new recurring transaction should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_AddsToCollection()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        virtualInstrument.AddRecurringTransaction("Monthly Savings", 500m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Single(virtualInstrument.RecurringTransactions);
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called
    /// Then the recurring transaction should have correct properties
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_SetsCorrectProperties()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var description = "Weekly Transfer";
        var amount = 100m;
        var schedule = ScheduleFrequency.Weekly;
        var nextRun = new DateOnly(2024, 6, 15);

        // Act
        var result = virtualInstrument.AddRecurringTransaction(description, amount, schedule, nextRun);

        // Assert
        Assert.Equal(description, result.Description);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(schedule, result.Schedule);
        Assert.Equal(nextRun, result.NextRun);
        Assert.Equal(virtualInstrument.Id, result.VirtualAccountId);
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called
    /// Then the created recurring transaction should be returned and present in the collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_ReturnsCreatedTransaction()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction("Test", 100m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.NotNull(result);
        Assert.Single(virtualInstrument.RecurringTransactions);
        Assert.Same(result, virtualInstrument.RecurringTransactions.First());
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called with null description
    /// Then the transaction should be created with null description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_NullDescription_AllowsNull()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction(null, 100m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Null(result.Description);
    }

    /// <summary>
    /// Given a virtual instrument with existing recurring transactions
    /// When AddRecurringTransaction is called
    /// Then the new transaction should be added to existing ones
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_AddsToExisting()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        virtualInstrument.AddRecurringTransaction("First", 100m, ScheduleFrequency.Weekly, DateOnly.FromDateTime(DateTime.Today));

        // Act
        virtualInstrument.AddRecurringTransaction("Second", 200m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Equal(2, virtualInstrument.RecurringTransactions.Count);
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called with different schedules
    /// Then the correct schedule should be set
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(ScheduleFrequency.Daily)]
    [InlineData(ScheduleFrequency.Weekly)]
    [InlineData(ScheduleFrequency.Fortnightly)]
    [InlineData(ScheduleFrequency.Monthly)]
    [InlineData(ScheduleFrequency.Yearly)]
    public void AddRecurringTransaction_DifferentSchedules_SetsCorrectSchedule(ScheduleFrequency schedule)
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction("Test", 100m, schedule, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Equal(schedule, result.Schedule);
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called with negative amount
    /// Then the transaction should be created with the negative amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_NegativeAmount_AllowsNegative()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction("Withdrawal", -50m, ScheduleFrequency.Weekly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Equal(-50m, result.Amount);
    }

    /// <summary>
    /// Given a virtual instrument
    /// When AddRecurringTransaction is called with zero amount
    /// Then the transaction should be created with zero amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddRecurringTransaction_ZeroAmount_AllowsZero()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act
        var result = virtualInstrument.AddRecurringTransaction("Placeholder", 0m, ScheduleFrequency.Monthly, DateOnly.FromDateTime(DateTime.Today));

        // Assert
        Assert.Equal(0m, result.Amount);
    }

    #endregion

    #region RemoveRecurringTransaction

    /// <summary>
    /// Given a virtual instrument with a recurring transaction
    /// When RemoveRecurringTransaction is called
    /// Then the recurring transaction should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveRecurringTransaction_ExistingTransaction_RemovesFromCollection()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var recurring = CreateRecurringTransaction(virtualInstrument, "Rent", 1500m);

        // Act
        virtualInstrument.RemoveRecurringTransaction(recurring.Id);

        // Assert
        Assert.Empty(virtualInstrument.RecurringTransactions);
    }

    /// <summary>
    /// Given a virtual instrument with multiple recurring transactions
    /// When RemoveRecurringTransaction is called
    /// Then only the requested recurring transaction should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveRecurringTransaction_MultipleTransactions_RemovesOnlyRequested()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var toRemove = CreateRecurringTransaction(virtualInstrument, "Rent", 1500m);
        var toKeep = CreateRecurringTransaction(virtualInstrument, "Utilities", 200m);

        // Act
        virtualInstrument.RemoveRecurringTransaction(toRemove.Id);

        // Assert
        Assert.Single(virtualInstrument.RecurringTransactions);
        Assert.Contains(toKeep, virtualInstrument.RecurringTransactions);
    }

    /// <summary>
    /// Given a virtual instrument without the requested recurring transaction
    /// When RemoveRecurringTransaction is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveRecurringTransaction_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();

        // Act & Assert
        Assert.Throws<NotFoundException>(() => virtualInstrument.RemoveRecurringTransaction(Guid.NewGuid()));
    }

    #endregion

    #region AdjustBalance

    /// <summary>
    /// Given a virtual instrument with a balance of 500
    /// When AdjustBalance is called with a higher balance of 800
    /// Then a balance adjustment event is raised for +300 and the balance is not set directly.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AdjustBalance_HigherBalance_RaisesPositiveAdjustmentEvent()
    {
        // Arrange
        var virtualInstrument = new DomainVirtualInstrument(Guid.NewGuid())
        {
            Name = "Test Virtual Instrument",
            Currency = "AUD",
            Balance = 500m,
        };

        // Act
        virtualInstrument.AdjustBalance(800m, "Web");

        // Assert
        var domainEvent = Assert.IsType<BalanceAdjustmentEvent>(Assert.Single(virtualInstrument.Events));
        Assert.Equal(300m, domainEvent.Amount);
        Assert.Equal("Web", domainEvent.Source);
        Assert.Same(virtualInstrument, domainEvent.Instrument);
        Assert.Equal(500m, virtualInstrument.Balance);
    }

    /// <summary>
    /// Given a virtual instrument with a balance of 1000
    /// When AdjustBalance is called with a lower balance of 800
    /// Then a balance adjustment event is raised for -200.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AdjustBalance_LowerBalance_RaisesNegativeAdjustmentEvent()
    {
        // Arrange
        var virtualInstrument = new DomainVirtualInstrument(Guid.NewGuid())
        {
            Name = "Test Virtual Instrument",
            Currency = "AUD",
            Balance = 1000m,
        };

        // Act
        virtualInstrument.AdjustBalance(800m, "Web");

        // Assert
        var domainEvent = Assert.IsType<BalanceAdjustmentEvent>(Assert.Single(virtualInstrument.Events));
        Assert.Equal(-200m, domainEvent.Amount);
    }

    #endregion

    private static RecurringTransaction CreateRecurringTransaction(DomainVirtualInstrument virtualInstrument, string description, decimal amount)
    {
        var recurring = new RecurringTransaction(Guid.NewGuid())
        {
            VirtualAccountId = virtualInstrument.Id,
            Description = description,
            Amount = amount,
            Schedule = ScheduleFrequency.Monthly,
            NextRun = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        virtualInstrument.RecurringTransactions.Add(recurring);
        return recurring;
    }

    private DomainVirtualInstrument CreateVirtualInstrument()
    {
        return new DomainVirtualInstrument(Guid.NewGuid())
        {
            Name = "Test Virtual Instrument",
            Currency = "AUD",
            ParentInstrumentId = Guid.NewGuid(),
        };
    }
}
