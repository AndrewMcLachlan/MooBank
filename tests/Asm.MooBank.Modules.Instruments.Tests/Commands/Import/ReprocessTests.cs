#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Import;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Import;

[Trait("Category", "Unit")]
public class ReprocessTests
{
    private readonly TestMocks _mocks;

    public ReprocessTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_QueuesReprocess()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var handler = new ReprocessHandler(_mocks.ReprocessQueueMock.Object);
        var command = new Reprocess(instrumentId, accountId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.ReprocessQueueMock.Verify(
            q => q.QueueReprocessTransactions(instrumentId, accountId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectInstrumentId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        Guid? capturedInstrumentId = null;

        _mocks.ReprocessQueueMock
            .Setup(q => q.QueueReprocessTransactions(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Callback<Guid, Guid>((iid, _) => capturedInstrumentId = iid);

        var handler = new ReprocessHandler(_mocks.ReprocessQueueMock.Object);
        var command = new Reprocess(instrumentId, accountId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, capturedInstrumentId);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectAccountId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        Guid? capturedAccountId = null;

        _mocks.ReprocessQueueMock
            .Setup(q => q.QueueReprocessTransactions(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Callback<Guid, Guid>((_, aid) => capturedAccountId = aid);

        var handler = new ReprocessHandler(_mocks.ReprocessQueueMock.Object);
        var command = new Reprocess(instrumentId, accountId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(accountId, capturedAccountId);
    }

    [Fact]
    public async Task Handle_CompletesImmediately()
    {
        // Arrange
        var handler = new ReprocessHandler(_mocks.ReprocessQueueMock.Object);
        var command = new Reprocess(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var task = handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert - Handler returns completed ValueTask
        Assert.True(task.IsCompleted);
    }
}
