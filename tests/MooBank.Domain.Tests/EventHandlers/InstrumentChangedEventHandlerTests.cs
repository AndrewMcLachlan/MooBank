using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.EventHandlers;
using Asm.MooBank.Domain.Entities.Instrument.Events;

namespace Asm.MooBank.Domain.Tests.EventHandlers;

/// <summary>
/// Unit tests for the <see cref="InstrumentChangedEventHandler"/> class.
/// Tests verify that LastUpdated is set correctly for created and updated instruments.
/// </summary>
public class InstrumentChangedEventHandlerTests
{
    private readonly InstrumentChangedEventHandler _handler;

    public InstrumentChangedEventHandlerTests()
    {
        _handler = new InstrumentChangedEventHandler();
    }

    #region InstrumentCreatedEvent

    /// <summary>
    /// Given an InstrumentCreatedEvent
    /// When Handle is called
    /// Then the instrument's LastUpdated should be set to UtcNow
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleCreated_SetsLastUpdatedToUtcNow()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var originalLastUpdated = instrument.LastUpdated;
        var beforeHandle = DateTimeOffset.UtcNow;
        var domainEvent = new InstrumentCreatedEvent(instrument);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);
        var afterHandle = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(instrument.LastUpdated >= beforeHandle);
        Assert.True(instrument.LastUpdated <= afterHandle);
        Assert.NotEqual(originalLastUpdated, instrument.LastUpdated);
    }

    /// <summary>
    /// Given an InstrumentCreatedEvent with an instrument that has default LastUpdated
    /// When Handle is called
    /// Then the instrument's LastUpdated should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleCreated_WithDefaultLastUpdated_SetsLastUpdated()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        Assert.Equal(default, instrument.LastUpdated);
        var domainEvent = new InstrumentCreatedEvent(instrument);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(default, instrument.LastUpdated);
    }

    #endregion

    #region InstrumentUpdatedEvent

    /// <summary>
    /// Given an InstrumentUpdatedEvent
    /// When Handle is called
    /// Then the instrument's LastUpdated should be set to UtcNow
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleUpdated_SetsLastUpdatedToUtcNow()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        instrument.LastUpdated = DateTimeOffset.UtcNow.AddDays(-7);
        var originalLastUpdated = instrument.LastUpdated;
        var beforeHandle = DateTimeOffset.UtcNow;
        var domainEvent = new InstrumentUpdatedEvent(instrument);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);
        var afterHandle = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(instrument.LastUpdated >= beforeHandle);
        Assert.True(instrument.LastUpdated <= afterHandle);
        Assert.NotEqual(originalLastUpdated, instrument.LastUpdated);
    }

    /// <summary>
    /// Given an InstrumentUpdatedEvent with a recently updated instrument
    /// When Handle is called
    /// Then the instrument's LastUpdated should still be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleUpdated_WithRecentLastUpdated_StillUpdatesLastUpdated()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        instrument.LastUpdated = DateTimeOffset.UtcNow.AddMilliseconds(-100);
        var previousLastUpdated = instrument.LastUpdated;
        var domainEvent = new InstrumentUpdatedEvent(instrument);

        // Act
        await Task.Delay(10, TestContext.Current.CancellationToken); // Small delay to ensure time difference
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(instrument.LastUpdated >= previousLastUpdated);
    }

    #endregion

    #region Multiple Calls

    /// <summary>
    /// Given the same instrument
    /// When Handle is called multiple times for InstrumentUpdatedEvent
    /// Then LastUpdated should be updated each time
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleUpdated_MultipleTimes_UpdatesLastUpdatedEachTime()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new InstrumentUpdatedEvent(instrument);

        // Act - First update
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);
        var firstUpdate = instrument.LastUpdated;

        await Task.Delay(10, TestContext.Current.CancellationToken);

        // Act - Second update
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);
        var secondUpdate = instrument.LastUpdated;

        // Assert
        Assert.True(secondUpdate >= firstUpdate);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given an InstrumentCreatedEvent
    /// When Handle is called and completes
    /// Then it should return a completed ValueTask
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleCreated_ReturnsCompletedValueTask()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new InstrumentCreatedEvent(instrument);

        // Act
        var task = _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(task.IsCompleted);
        await task; // Should not throw
    }

    /// <summary>
    /// Given an InstrumentUpdatedEvent
    /// When Handle is called and completes
    /// Then it should return a completed ValueTask
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HandleUpdated_ReturnsCompletedValueTask()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new InstrumentUpdatedEvent(instrument);

        // Act
        var task = _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(task.IsCompleted);
        await task; // Should not throw
    }

    #endregion

    private LogicalAccount CreateLogicalAccount()
    {
        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = Guid.NewGuid() }],
        };
    }
}
