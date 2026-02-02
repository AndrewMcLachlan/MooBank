using Asm.MooBank.Domain.Entities.Institution;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Commands;
using Asm.MooBank.Modules.Institutions.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Institutions.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsInstitution()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("New Bank", InstitutionType.Bank);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Bank", result.Name);
        Assert.Equal(InstitutionType.Bank, result.InstitutionType);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        Institution? capturedEntity = null;
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Add(It.IsAny<Institution>()))
            .Callback<Institution>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("New Bank", InstitutionType.Bank);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.InstitutionRepositoryMock.Verify(r => r.Add(It.IsAny<Institution>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("New Bank", capturedEntity.Name);
        Assert.Equal(InstitutionType.Bank, capturedEntity.InstitutionType);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("New Bank", InstitutionType.Bank);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(InstitutionType.Bank)]
    [InlineData(InstitutionType.CreditUnion)]
    [InlineData(InstitutionType.BuildingSociety)]
    [InlineData(InstitutionType.SuperannuationFund)]
    [InlineData(InstitutionType.Broker)]
    [InlineData(InstitutionType.Other)]
    public async Task Handle_DifferentInstitutionTypes_SetsCorrectType(InstitutionType expectedType)
    {
        // Arrange
        Institution? capturedEntity = null;
        _mocks.InstitutionRepositoryMock
            .Setup(r => r.Add(It.IsAny<Institution>()))
            .Callback<Institution>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("Test Institution", expectedType);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(expectedType, result.InstitutionType);
        Assert.Equal(expectedType, capturedEntity.InstitutionType);
    }

    [Fact]
    public async Task Handle_NonAdminUser_ThrowsNotAuthorisedException()
    {
        // Arrange
        _mocks.SecurityFailAdmin();

        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("New Bank", InstitutionType.Bank);

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_NonAdminUser_DoesNotAddToRepository()
    {
        // Arrange
        _mocks.SecurityFailAdmin();

        var handler = new CreateHandler(
            _mocks.InstitutionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.SecurityMock.Object);
        var command = new Create("New Bank", InstitutionType.Bank);

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
        _mocks.InstitutionRepositoryMock.Verify(r => r.Add(It.IsAny<Institution>()), Times.Never);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
