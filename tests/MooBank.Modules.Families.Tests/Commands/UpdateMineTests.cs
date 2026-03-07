#nullable enable
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Models;
using Asm.MooBank.Modules.Families.Tests.Support;

namespace Asm.MooBank.Modules.Families.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateMineTests
{
    private readonly TestMocks _mocks;

    public UpdateMineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedFamily()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateMineHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new UpdateMine(updateFamily);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateMineHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new UpdateMine(updateFamily);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", existingFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateMineHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new UpdateMine(updateFamily);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_FetchesUserFamily()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "My Family");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateMineHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new UpdateMine(updateFamily);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Get(familyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentUser_UpdatesDifferentFamily()
    {
        // Arrange
        var differentFamilyId = Guid.NewGuid();
        var differentUser = TestMocks.CreateTestUser(familyId: differentFamilyId);
        _mocks.SetUser(differentUser);

        var existingFamily = TestEntities.CreateFamily(id: differentFamilyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(differentFamilyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateMineHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User);

        var updateFamily = new UpdateFamily { Name = "Different Family Name" };
        var command = new UpdateMine(updateFamily);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Different Family Name", result.Name);
        _mocks.FamilyRepositoryMock.Verify(r => r.Get(differentFamilyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
