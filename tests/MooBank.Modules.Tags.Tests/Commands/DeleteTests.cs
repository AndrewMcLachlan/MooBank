#nullable enable
using Asm.MooBank.Modules.Tags.Commands;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Commands;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Tag to Delete", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var handler = new DeleteHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var command = new Delete(1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.TagRepositoryMock.Verify(r => r.Delete(existingTag), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var existingTag = TestEntities.CreateTag(id: 1, name: "Tag to Delete", familyId: familyId);

        _mocks.TagRepositoryMock
            .Setup(r => r.Get(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var handler = new DeleteHandler(
            _mocks.TagRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var command = new Delete(1);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.AuditingUnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}
