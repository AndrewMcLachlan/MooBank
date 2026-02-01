#nullable enable
using Asm.MooBank.Modules.Families.Commands;
using Asm.MooBank.Modules.Families.Tests.Support;
using DomainFamily = Asm.MooBank.Domain.Entities.Family.Family;

namespace Asm.MooBank.Modules.Families.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedFamily()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Family", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainFamily? capturedFamily = null;

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Add(It.IsAny<DomainFamily>()), Times.Once);
        Assert.NotNull(capturedFamily);
        Assert.Equal("New Family", capturedFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_AssertAdministrator()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

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

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NonAdmin_DoesNotAddToRepository()
    {
        // Arrange
        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotAuthorisedException());

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);

        var command = new Create("New Family");

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
        _mocks.FamilyRepositoryMock.Verify(r => r.Add(It.IsAny<DomainFamily>()), Times.Never);
    }
}
