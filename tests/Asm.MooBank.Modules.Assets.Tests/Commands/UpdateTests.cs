#nullable enable
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Modules.Assets.Commands;
using Asm.MooBank.Modules.Assets.Tests.Support;
using Asm.Security;

namespace Asm.MooBank.Modules.Assets.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsAsset()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(
            id: assetId,
            name: "Old Name",
            description: "Old description",
            value: 10000m);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Updated Name",
            Description = "Updated description",
            CurrentBalance = 15000m,
            PurchasePrice = 8000m,
            ShareWithFamily = true,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(15000m, result.CurrentBalance);
        Assert.Equal(8000m, result.PurchasePrice);
        Assert.True(result.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_ModifiesEntityProperties()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(
            id: assetId,
            name: "Original",
            description: "Original desc",
            value: 5000m,
            purchasePrice: 3000m,
            shareWithFamily: false);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Modified",
            Description = "Modified desc",
            CurrentBalance = 8000m,
            PurchasePrice = 4000m,
            ShareWithFamily = true,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("Modified", existingEntity.Name);
        Assert.Equal("Modified desc", existingEntity.Description);
        Assert.Equal(8000m, existingEntity.Value);
        Assert.Equal(4000m, existingEntity.PurchasePrice);
        Assert.True(existingEntity.ShareWithFamily);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(id: assetId);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(id: assetId);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.AssetRepositoryMock.Verify(r => r.Update(existingEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(id: assetId);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = groupId,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(groupId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGroupId_NoPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        _mocks.SecurityFailGroupPermission();

        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(id: assetId);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = Guid.NewGuid(),
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_WithGroupId_NoPermission_DoesNotFetchFromRepository()
    {
        // Arrange
        _mocks.SecurityFailGroupPermission();

        var assetId = Guid.NewGuid();

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = Guid.NewGuid(),
        };

        // Act
        try
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        }
        catch (NotAuthorisedException)
        {
            // Expected
        }

        // Assert
        _mocks.AssetRepositoryMock.Verify(
            r => r.Get(It.IsAny<Guid>(), It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutGroupId_DoesNotCheckGroupPermission()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(id: assetId);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
            GroupId = null,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentAsset_ThrowsNotFoundException()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Asset>(null!));

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            ShareWithFamily = false,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ClearPurchasePrice_SetsNullPurchasePrice()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var existingEntity = TestEntities.CreateAsset(
            id: assetId,
            purchasePrice: 5000m);

        _mocks.AssetRepositoryMock
            .Setup(r => r.Get(assetId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        var handler = new UpdateHandler(
            _mocks.AssetRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            AccountId = assetId,
            Name = "Test",
            Description = "Test",
            CurrentBalance = 1000m,
            PurchasePrice = null,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(existingEntity.PurchasePrice);
    }
}
