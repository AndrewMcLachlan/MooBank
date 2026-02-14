#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class RunTests
{
    private readonly TestMocks _mocks;

    public RunTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_QueuesRuleRun()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        var handler = new RunHandler(_mocks.RunRulesQueueMock.Object);
        var command = new Run(instrumentId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.RunRulesQueueMock.Verify(
            q => q.QueueRunRules(instrumentId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCorrectInstrumentId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        Guid? capturedInstrumentId = null;

        _mocks.RunRulesQueueMock
            .Setup(q => q.QueueRunRules(It.IsAny<Guid>()))
            .Callback<Guid>(iid => capturedInstrumentId = iid);

        var handler = new RunHandler(_mocks.RunRulesQueueMock.Object);
        var command = new Run(instrumentId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, capturedInstrumentId);
    }

    [Fact]
    public async Task Handle_CompletesImmediately()
    {
        // Arrange
        var handler = new RunHandler(_mocks.RunRulesQueueMock.Object);
        var command = new Run(Guid.NewGuid());

        // Act
        var task = handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert - Handler returns completed ValueTask
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task Handle_MultipleInstruments_QueuesEachSeparately()
    {
        // Arrange
        var instrumentId1 = Guid.NewGuid();
        var instrumentId2 = Guid.NewGuid();
        var queuedIds = new List<Guid>();

        _mocks.RunRulesQueueMock
            .Setup(q => q.QueueRunRules(It.IsAny<Guid>()))
            .Callback<Guid>(iid => queuedIds.Add(iid));

        var handler = new RunHandler(_mocks.RunRulesQueueMock.Object);

        // Act
        await handler.Handle(new Run(instrumentId1), TestContext.Current.CancellationToken);
        await handler.Handle(new Run(instrumentId2), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, queuedIds.Count);
        Assert.Contains(instrumentId1, queuedIds);
        Assert.Contains(instrumentId2, queuedIds);
    }
}
