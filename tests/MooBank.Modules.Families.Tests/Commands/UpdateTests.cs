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

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

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
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", existingFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var existingFamily = TestEntities.CreateFamily(id: familyId, name: "Old Name");

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Get(familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFamily);

        var handler = new UpdateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updateFamily = new UpdateFamily { Name = "New Name" };
        var command = new Update(familyId, updateFamily);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

}
