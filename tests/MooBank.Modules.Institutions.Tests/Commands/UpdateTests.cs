using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Commands;
using Asm.MooBank.Modules.Institutions.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Institutions.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsInstitution()
    {
        // Arrange
        var existingInstitution = TestEntities.CreateInstitution(1, "Old Name", InstitutionType.Bank);
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInstitution);

        var handler = new UpdateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);
        var command = new Update(1, "New Name", InstitutionType.CreditUnion);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(InstitutionType.CreditUnion, result.InstitutionType);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var existingInstitution = TestEntities.CreateInstitution(1, "Old Name", InstitutionType.Bank);
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInstitution);

        var handler = new UpdateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);
        var command = new Update(1, "Updated Name", InstitutionType.BuildingSociety);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Updated Name", existingInstitution.Name);
        Assert.Equal(InstitutionType.BuildingSociety, existingInstitution.InstitutionType);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var existingInstitution = TestEntities.CreateInstitution(1, "Old Name", InstitutionType.Bank);
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Get(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInstitution);

        var handler = new UpdateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);
        var command = new Update(1, "New Name", InstitutionType.CreditUnion);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentInstitution_ThrowsNotFoundException()
    {
        // Arrange
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Get(999, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new UpdateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);
        var command = new Update(999, "New Name", InstitutionType.Bank);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
