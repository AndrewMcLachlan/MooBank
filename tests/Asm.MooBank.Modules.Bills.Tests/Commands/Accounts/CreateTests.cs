#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Commands.Accounts;
using Asm.MooBank.Modules.Bills.Tests.Support;
using DomainAccount = Asm.MooBank.Domain.Entities.Utility.Account;

namespace Asm.MooBank.Modules.Bills.Tests.Commands.Accounts;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsAccount()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "New Electricity Account",
            Description = "Test description",
            UtilityType = UtilityType.Electricity,
            AccountNumber = "ELEC123",
            Currency = "AUD",
            ShareWithFamily = true,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Electricity Account", result.Name);
        Assert.Equal(UtilityType.Electricity, result.UtilityType);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Gas Account",
            Description = "Monthly gas",
            UtilityType = UtilityType.Gas,
            AccountNumber = "GAS456",
            InstitutionId = 5,
            Currency = "AUD",
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.AccountRepositoryMock.Verify(r => r.Add(It.IsAny<DomainAccount>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Gas Account", capturedEntity.Name);
        Assert.Equal("Monthly gas", capturedEntity.Description);
        Assert.Equal(UtilityType.Gas, capturedEntity.UtilityType);
        Assert.Equal("GAS456", capturedEntity.AccountNumber);
        Assert.Equal(5, capturedEntity.InstitutionId);
        Assert.False(capturedEntity.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test",
            UtilityType = UtilityType.Water,
            AccountNumber = "WATER001",
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAccountHolder()
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test",
            UtilityType = UtilityType.Electricity,
            AccountNumber = "TEST001",
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Single(capturedEntity.Owners);
        Assert.Equal(_mocks.User.Id, capturedEntity.Owners.First().UserId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsControllerToManual()
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test",
            UtilityType = UtilityType.Internet,
            AccountNumber = "NET001",
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Equal(Controller.Manual, capturedEntity.Controller);
    }

    [Theory]
    [InlineData(UtilityType.Electricity)]
    [InlineData(UtilityType.Gas)]
    [InlineData(UtilityType.Water)]
    [InlineData(UtilityType.Phone)]
    [InlineData(UtilityType.Internet)]
    [InlineData(UtilityType.Other)]
    public async Task Handle_DifferentUtilityTypes_SetsCorrectType(UtilityType expectedType)
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test Account",
            UtilityType = expectedType,
            AccountNumber = "TEST001",
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedType, result.UtilityType);
        Assert.Equal(expectedType, capturedEntity!.UtilityType);
    }

    [Fact]
    public async Task Handle_WithNullInstitutionId_SetsNullInstitutionId()
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test",
            UtilityType = UtilityType.Other,
            AccountNumber = "OTHER001",
            InstitutionId = null,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(capturedEntity!.InstitutionId);
    }

    [Fact]
    public async Task Handle_DefaultCurrency_UsesAUD()
    {
        // Arrange
        DomainAccount? capturedEntity = null;
        _mocks.AccountRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainAccount>()))
            .Callback<DomainAccount>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.UnitOfWorkMock.Object,
            _mocks.AccountRepositoryMock.Object,
            _mocks.User);

        var command = new Create
        {
            Name = "Test",
            UtilityType = UtilityType.Electricity,
            AccountNumber = "TEST001",
            // Currency not specified, should default to AUD
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("AUD", capturedEntity!.Currency);
    }
}
