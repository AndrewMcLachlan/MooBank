#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Old Name", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateTag = TestEntities.CreateUpdateTag(name: "New Name");
        var command = new Update(1, updateTag);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesEntityName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Old Name", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateTag = TestEntities.CreateUpdateTag(name: "Updated Name");
        var command = new Update(1, updateTag);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Updated Name", existingTag.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Test", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateTag = TestEntities.CreateUpdateTag(name: "Test");
        var command = new Update(1, updateTag);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChecksFamilyPermission()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Test", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateTag = TestEntities.CreateUpdateTag(name: "Test");
        var command = new Update(1, updateTag);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertFamilyPermission(familyId), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesColour()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Test", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var newColour = new Asm.Drawing.HexColour("#00FF00");
        var updateTag = TestEntities.CreateUpdateTag(name: "Test", colour: newColour);
        var command = new Update(1, updateTag);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(newColour, existingTag.Colour);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesSettings()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Test", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        _mocks.SecurityMock
            .Setup(s => s.AssertFamilyPermission(familyId))
            .Returns(Task.CompletedTask);

        var handler = new UpdateHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateTag = TestEntities.CreateUpdateTag(
            name: "Test",
            applySmoothing: true,
            excludeFromReporting: true);
        var command = new Update(1, updateTag);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(existingTag.Settings.ApplySmoothing);
        Assert.True(existingTag.Settings.ExcludeFromReporting);
    }
}
