#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsTag()
    {
        // Arrange
        DomainTag? capturedEntity = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(e => capturedEntity = e)
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var tagModel = TestEntities.CreateTagModel(name: "Groceries");
        var command = new Create(tagModel);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Groceries", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainTag? capturedEntity = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(e => capturedEntity = e)
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var tagModel = TestEntities.CreateTagModel(name: "Fuel");
        var command = new Create(tagModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.TagRepositoryMock.Verify(r => r.Add(It.IsAny<DomainTag>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Fuel", capturedEntity.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var tagModel = TestEntities.CreateTagModel(name: "Test");
        var command = new Create(tagModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsFamilyIdFromUser()
    {
        // Arrange
        var specificFamilyId = Guid.NewGuid();
        var user = TestMocks.CreateTestUser(familyId: specificFamilyId);
        _mocks.SetUser(user);

        DomainTag? capturedEntity = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(e => capturedEntity = e)
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var tagModel = TestEntities.CreateTagModel(name: "Test");
        var command = new Create(tagModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(specificFamilyId, capturedEntity.FamilyId);
    }

    [Fact]
    public async Task Handle_WithSettings_SetsSettings()
    {
        // Arrange
        DomainTag? capturedEntity = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(e => capturedEntity = e)
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var tagModel = TestEntities.CreateTagModel(
            name: "Smoothed Tag",
            applySmoothing: true,
            excludeFromReporting: true);
        var command = new Create(tagModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.True(capturedEntity.Settings.ApplySmoothing);
        Assert.True(capturedEntity.Settings.ExcludeFromReporting);
    }

    [Fact]
    public async Task Handle_WithColour_SetsColour()
    {
        // Arrange
        DomainTag? capturedEntity = null;
        _mocks.TagRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTag>()))
            .Callback<DomainTag>(e => capturedEntity = e)
            .Returns<DomainTag>(e => e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.TagRepositoryMock.Object,
            _mocks.User);

        var colour = new Asm.Drawing.HexColour("#FF5733");
        var tagModel = TestEntities.CreateTagModel(name: "Coloured Tag", colour: colour);
        var command = new Create(tagModel);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(colour, capturedEntity.Colour);
    }
}
