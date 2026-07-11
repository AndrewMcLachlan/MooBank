#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// Given an instrument with a rule
    /// When the Delete command is handled
    /// Then the rule should be removed from the instrument
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_DeletesRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.DoesNotContain(instrument.Rules, r => r.Id == 1);
    }

    /// <summary>
    /// Given an instrument with a rule
    /// When the Delete command is handled
    /// Then changes should be saved
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given an instrument with multiple rules
    /// When the Delete command is handled
    /// Then only the requested rule should be removed
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_RemovesOnlyRequestedRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var rule1 = TestEntities.CreateRule(id: 1, instrumentId: instrumentId);
        var rule2 = TestEntities.CreateRule(id: 42, instrumentId: instrumentId);
        var instrument = TestEntities.CreateInstrument(id: instrumentId, rules: [rule1, rule2]);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 42);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(instrument.Rules);
        Assert.Contains(instrument.Rules, r => r.Id == 1);
    }

    /// <summary>
    /// Given an instrument without the requested rule
    /// When the Delete command is handled
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    public async Task Handle_RuleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var handler = new DeleteHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
