#nullable enable

using Asm.MooBank.Models;
using Asm.MooBank.Queues;
using Asm.MooBank.Services;

namespace Asm.MooBank.Core.Tests.Services;

/// <summary>
/// Unit tests for the queue implementations.
/// Tests cover queue/dequeue operations, disposal, and concurrency.
/// </summary>
public class ImportTransactionsQueueTests : IDisposable
{
    private readonly ImportTransactionsQueue _queue = new();

    public void Dispose()
    {
        _queue.Dispose();
        GC.SuppressFinalize(this);
    }

    #region QueueImport

    /// <summary>
    /// Given an empty queue
    /// When QueueImport is called
    /// Then the item should be queued successfully
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueImport_ToEmptyQueue_QueuesSuccessfully()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = CreateUser();
        var fileData = new byte[] { 1, 2, 3 };

        // Act - should not throw
        _queue.QueueImport(instrumentId, accountId, user, fileData);

        // Assert - if we got here without exception, it worked
        Assert.True(true);
    }

    /// <summary>
    /// Given multiple items queued
    /// When DequeueAsync is called multiple times
    /// Then items should be returned in FIFO order
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task QueueImport_MultipleItems_DequeuedInFifoOrder()
    {
        // Arrange
        var user = CreateUser();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        _queue.QueueImport(id1, Guid.NewGuid(), user, []);
        _queue.QueueImport(id2, Guid.NewGuid(), user, []);
        _queue.QueueImport(id3, Guid.NewGuid(), user, []);

        // Act
        var item1 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var item2 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var item3 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert - FIFO order
        Assert.Equal(id1, item1.InstrumentId);
        Assert.Equal(id2, item2.InstrumentId);
        Assert.Equal(id3, item3.InstrumentId);
    }

    #endregion

    #region DequeueAsync

    /// <summary>
    /// Given an item is queued
    /// When DequeueAsync is called
    /// Then the correct item should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_WithQueuedItem_ReturnsItem()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = CreateUser();
        var fileData = new byte[] { 1, 2, 3, 4, 5 };

        _queue.QueueImport(instrumentId, accountId, user, fileData);

        // Act
        var item = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, item.InstrumentId);
        Assert.Equal(accountId, item.AccountId);
        Assert.Equal(user.Id, item.User.Id);
        Assert.Equal(fileData, item.FileData);
    }

    /// <summary>
    /// Given a disposed queue
    /// When QueueImport is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueImport_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new ImportTransactionsQueue();
        queue.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            queue.QueueImport(Guid.NewGuid(), Guid.NewGuid(), CreateUser(), []));
    }

    /// <summary>
    /// Given a disposed queue
    /// When DequeueAsync is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new ImportTransactionsQueue();
        queue.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            queue.DequeueAsync(TestContext.Current.CancellationToken));
    }

    #endregion

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        Currency = "AUD",
        FamilyId = Guid.NewGuid(),
    };
}

/// <summary>
/// Unit tests for the RunRulesQueue.
/// </summary>
public class RunRulesQueueTests : IDisposable
{
    private readonly RunRulesQueue _queue = new();

    public void Dispose()
    {
        _queue.Dispose();
        GC.SuppressFinalize(this);
    }

    #region QueueRunRules

    /// <summary>
    /// Given an empty queue
    /// When QueueRunRules is called
    /// Then the item should be queued successfully
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueRunRules_ToEmptyQueue_QueuesSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act - should not throw
        _queue.QueueRunRules(accountId);

        // Assert
        Assert.True(true);
    }

    /// <summary>
    /// Given multiple items queued
    /// When DequeueAsync is called multiple times
    /// Then items should be returned in FIFO order
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task QueueRunRules_MultipleItems_DequeuedInFifoOrder()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        _queue.QueueRunRules(id1);
        _queue.QueueRunRules(id2);
        _queue.QueueRunRules(id3);

        // Act
        var item1 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var item2 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var item3 = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert - FIFO order
        Assert.Equal(id1, item1);
        Assert.Equal(id2, item2);
        Assert.Equal(id3, item3);
    }

    #endregion

    #region DequeueAsync

    /// <summary>
    /// Given an item is queued
    /// When DequeueAsync is called
    /// Then the correct item should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_WithQueuedItem_ReturnsItem()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _queue.QueueRunRules(accountId);

        // Act
        var result = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(accountId, result);
    }

    /// <summary>
    /// Given a disposed queue
    /// When QueueRunRules is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueRunRules_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new RunRulesQueue();
        queue.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => queue.QueueRunRules(Guid.NewGuid()));
    }

    /// <summary>
    /// Given a disposed queue
    /// When DequeueAsync is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new RunRulesQueue();
        queue.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            queue.DequeueAsync(TestContext.Current.CancellationToken));
    }

    #endregion
}

/// <summary>
/// Unit tests for the ReprocessTransactionsQueue.
/// </summary>
public class ReprocessTransactionsQueueTests : IDisposable
{
    private readonly ReprocessTransactionsQueue _queue = new();

    public void Dispose()
    {
        _queue.Dispose();
        GC.SuppressFinalize(this);
    }

    #region QueueReprocessTransactions

    /// <summary>
    /// Given an empty queue
    /// When QueueReprocessTransactions is called
    /// Then the item should be queued successfully
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueReprocessTransactions_ToEmptyQueue_QueuesSuccessfully()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        // Act - should not throw
        _queue.QueueReprocessTransactions(instrumentId, accountId);

        // Assert
        Assert.True(true);
    }

    /// <summary>
    /// Given multiple items queued
    /// When DequeueAsync is called multiple times
    /// Then items should be returned in FIFO order
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task QueueReprocessTransactions_MultipleItems_DequeuedInFifoOrder()
    {
        // Arrange
        var inst1 = Guid.NewGuid();
        var inst2 = Guid.NewGuid();
        var inst3 = Guid.NewGuid();

        _queue.QueueReprocessTransactions(inst1, Guid.NewGuid());
        _queue.QueueReprocessTransactions(inst2, Guid.NewGuid());
        _queue.QueueReprocessTransactions(inst3, Guid.NewGuid());

        // Act
        var (instId1, _) = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var (instId2, _) = await _queue.DequeueAsync(TestContext.Current.CancellationToken);
        var (instId3, _) = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert - FIFO order
        Assert.Equal(inst1, instId1);
        Assert.Equal(inst2, instId2);
        Assert.Equal(inst3, instId3);
    }

    #endregion

    #region DequeueAsync

    /// <summary>
    /// Given an item is queued
    /// When DequeueAsync is called
    /// Then the correct item should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_WithQueuedItem_ReturnsItem()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _queue.QueueReprocessTransactions(instrumentId, accountId);

        // Act
        var (resultInstrumentId, resultAccountId) = await _queue.DequeueAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, resultInstrumentId);
        Assert.Equal(accountId, resultAccountId);
    }

    /// <summary>
    /// Given a disposed queue
    /// When QueueReprocessTransactions is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void QueueReprocessTransactions_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new ReprocessTransactionsQueue();
        queue.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
            queue.QueueReprocessTransactions(Guid.NewGuid(), Guid.NewGuid()));
    }

    /// <summary>
    /// Given a disposed queue
    /// When DequeueAsync is called
    /// Then ObjectDisposedException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task DequeueAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var queue = new ReprocessTransactionsQueue();
        queue.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            queue.DequeueAsync(TestContext.Current.CancellationToken));
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Given a queue
    /// When Dispose is called multiple times
    /// Then it should not throw
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var queue = new ReprocessTransactionsQueue();

        // Act & Assert - should not throw
        queue.Dispose();
        queue.Dispose();
        queue.Dispose();
    }

    #endregion
}
