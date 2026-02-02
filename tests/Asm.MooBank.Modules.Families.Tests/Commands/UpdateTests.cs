#nullable enable
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Models;
using Asm.MooBank.Modules.Families.Tests.Support;
using DomainFamily = Asm.MooBank.Domain.Entities.Family.Family;

namespace Asm.MooBank.Modules.Families.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityName()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", existingFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssertAdministrator()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertAdministrator(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonAdmin_ThrowsNotAuthorisedException()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(Guid.NewGuid(), updateFamily);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NonAdmin_DoesNotFetchFromRepository()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(Guid.NewGuid(), updateFamily);

        // Act
        try
        {
            await handler.Handle(command, CancellationToken.None);
        }
        catch (NotAuthorisedException)
        {
            // Expected
        }

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
