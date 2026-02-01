#nullable enable
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Modules.Assets.Commands;
using Asm.MooBank.Modules.Assets.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Assets.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsAsset()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "New Asset",
            Description = "Test asset description",
            CurrentBalance = 50000m,
            PurchasePrice = 40000m,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Asset", result.Name);
        Assert.Equal("Test asset description", result.Description);
        Assert.Equal(50000m, result.CurrentBalance);
        Assert.Equal(40000m, result.PurchasePrice);
        Assert.False(result.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        Asset? capturedEntity = null;
        _mocks.AssetRepositoryMock
            .Setup(r => r.Add(It.IsAny<Asset>()))
            .Callback<Asset>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Test Asset",
            Description = "Description",
            CurrentBalance = 25000m,
            PurchasePrice = 20000m,
            ShareWithFamily = true,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.AssetRepositoryMock.Verify(r => r.Add(It.IsAny<Asset>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Test Asset", capturedEntity.Name);
        Assert.Equal("Description", capturedEntity.Description);
        Assert.Equal(25000m, capturedEntity.Value);
        Assert.Equal(20000m, capturedEntity.PurchasePrice);
        Assert.True(capturedEntity.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAccountHolder()
    {
        // Arrange
        Asset? capturedEntity = null;
        _mocks.AssetRepositoryMock
            .Setup(r => r.Add(It.IsAny<Asset>()))
            .Callback<Asset>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Single(capturedEntity.Owners);
        Assert.Equal(_mocks.User.Id, capturedEntity.Owners.First().UserId);
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Group Asset",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = groupId,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(groupId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGroupId_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        _mocks.SecurityFailGroupPermission();

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Group Asset",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = Guid.NewGuid(),
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WithoutGroupId_DoesNotCheckGroupPermission()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Personal Asset",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = null,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullPurchasePrice_SetsNullPurchasePrice()
    {
        // Arrange
        Asset? capturedEntity = null;
        _mocks.AssetRepositoryMock
            .Setup(r => r.Add(It.IsAny<Asset>()))
            .Callback<Asset>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Gift Asset",
            Description = "Received as gift",
            CurrentBalance = 5000m,
            PurchasePrice = null,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Null(result.PurchasePrice);
        Assert.Null(capturedEntity!.PurchasePrice);
    }

    [Fact]
    public async Task Handle_ShareWithFamilyTrue_SetsShareWithFamily()
    {
        // Arrange
        Asset? capturedEntity = null;
        _mocks.AssetRepositoryMock
            .Setup(r => r.Add(It.IsAny<Asset>()))
            .Callback<Asset>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Family Asset",
            Description = "Shared with family",
            CurrentBalance = 100000m,
            ShareWithFamily = true,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.ShareWithFamily);
        Assert.True(capturedEntity!.ShareWithFamily);
    }
}
