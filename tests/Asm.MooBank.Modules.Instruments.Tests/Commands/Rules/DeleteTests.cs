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

    [Fact]
    public async Task Handle_ValidCommand_DeletesRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        var handler = new DeleteHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.RuleRepositoryMock.Verify(r => r.Delete(instrumentId, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        var handler = new DeleteHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, 1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsDeleteWithCorrectParameters()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var ruleId = 42;

        var handler = new DeleteHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Delete(instrumentId, ruleId);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.RuleRepositoryMock.Verify(r => r.Delete(instrumentId, ruleId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
