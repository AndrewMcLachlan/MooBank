#nullable enable
using Asm.Drawing;
using Asm.MooBank.Modules.Groups.Commands;
using Asm.MooBank.Modules.Groups.Tests.Support;
using DomainGroup = Asm.MooBank.Domain.Entities.Group.Group;

namespace Asm.MooBank.Modules.Groups.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedGroup()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", true);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Group", result.Name);
        Assert.Equal("A test group", result.Description);
        Assert.True(result.ShowTotal);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.GroupRepositoryMock.Verify(r => r.Add(It.IsAny<DomainGroup>()), Times.Once);
        Assert.NotNull(capturedGroup);
        Assert.Equal("New Group", capturedGroup.Name);
        Assert.Equal("A test group", capturedGroup.Description);
        Assert.False(capturedGroup.ShowPosition);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsOwnerToCurrentUser()
    {
        // Arrange
        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedGroup);
        Assert.Equal(_mocks.User.Id, capturedGroup.OwnerId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShowTotalTrue_MapsToShowPositionTrue()
    {
        // Arrange
        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", true);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedGroup);
        Assert.True(capturedGroup.ShowPosition);
    }

    /// <summary>
    /// Given a create command with a colour
    /// When the handler is invoked
    /// Then the created entity and returned model should carry the supplied colour
    /// </summary>
    [Fact]
    public async Task Handle_CommandWithColour_SetsColour()
    {
        // Arrange
        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var colour = new HexColour("#ff7c43");
        var command = new Create("New Group", "A test group", false, colour);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedGroup);
        Assert.Equal(colour, capturedGroup.Colour);
        Assert.Equal(colour, result.Colour);
    }

    /// <summary>
    /// Given a create command without a colour
    /// When the handler is invoked
    /// Then the created entity should have no colour
    /// </summary>
    [Fact]
    public async Task Handle_CommandWithoutColour_LeavesColourNull()
    {
        // Arrange
        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", false);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedGroup);
        Assert.Null(capturedGroup.Colour);
        Assert.Null(result.Colour);
    }

    [Fact]
    public async Task Handle_DifferentUser_SetsCorrectOwner()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var differentUser = TestMocks.CreateTestUser(id: differentUserId);
        _mocks.SetUser(differentUser);

        DomainGroup? capturedGroup = null;

        _mocks.GroupRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainGroup>()))
            .Callback<DomainGroup>(g => capturedGroup = g);

        var handler = new CreateHandler(
            _mocks.GroupRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var command = new Create("New Group", "A test group", false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedGroup);
        Assert.Equal(differentUserId, capturedGroup.OwnerId);
    }
}
