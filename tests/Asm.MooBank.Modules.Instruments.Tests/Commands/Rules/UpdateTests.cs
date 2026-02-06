#nullable enable
using Asm.MooBank.Modules.Instruments.Commands.Rules;
using Asm.MooBank.Modules.Instruments.Tests.Support;
using DomainRule = Asm.MooBank.Domain.Entities.Instrument.Rule;

namespace Asm.MooBank.Modules.Instruments.Tests.Commands.Rules;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedRule()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingRule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "OLD_VALUE");

        _mocks.RuleRepositoryMock
            .Setup(r => r.Get(instrumentId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var handler = new UpdateRuleHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateRule = TestEntities.CreateUpdateRule(contains: "NEW_VALUE", description: "New Description");
        var command = new Update(instrumentId, 1, updateRule);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW_VALUE", result.Contains);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingRule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, contains: "OLD_VALUE", description: "Old Description");

        _mocks.RuleRepositoryMock
            .Setup(r => r.Get(instrumentId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var handler = new UpdateRuleHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateRule = TestEntities.CreateUpdateRule(contains: "NEW_VALUE", description: "New Description");
        var command = new Update(instrumentId, 1, updateRule);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("NEW_VALUE", existingRule.Contains);
        Assert.Equal("New Description", existingRule.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingRule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId);

        _mocks.RuleRepositoryMock
            .Setup(r => r.Get(instrumentId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var handler = new UpdateRuleHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateRule = TestEntities.CreateUpdateRule(contains: "NEW_VALUE");
        var command = new Update(instrumentId, 1, updateRule);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullDescription_SetsDescriptionToNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingRule = TestEntities.CreateRule(id: 1, instrumentId: instrumentId, description: "Has Description");

        _mocks.RuleRepositoryMock
            .Setup(r => r.Get(instrumentId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRule);

        var handler = new UpdateRuleHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        // Create UpdateRule directly to explicitly set null description
        var updateRule = new Instruments.Models.Rules.UpdateRule
        {
            Contains = "VALUE",
            Description = null,
            Tags = [],
        };
        var command = new Update(instrumentId, 1, updateRule);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(existingRule.Description);
    }

    [Fact]
    public async Task Handle_RuleNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonExistentRuleId = 999;

        _mocks.RuleRepositoryMock
            .Setup(r => r.Get(instrumentId, nonExistentRuleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new UpdateRuleHandler(
            _mocks.RuleRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateRule = TestEntities.CreateUpdateRule(contains: "NEW_VALUE");
        var command = new Update(instrumentId, nonExistentRuleId, updateRule);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }
}
