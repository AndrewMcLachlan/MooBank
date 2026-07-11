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
        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var command = new Create("New Family");

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Family", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainFamily? capturedFamily = null;

        _mocks.FamilyRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainFamily>()))
            .Callback<DomainFamily>(f => capturedFamily = f);

        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var command = new Create("New Family");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.FamilyRepositoryMock.Verify(r => r.Add(It.IsAny<DomainFamily>()), Times.Once);
        Assert.NotNull(capturedFamily);
        Assert.Equal("New Family", capturedFamily.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.FamilyRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

        var command = new Create("New Family");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.AuditingUnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}
