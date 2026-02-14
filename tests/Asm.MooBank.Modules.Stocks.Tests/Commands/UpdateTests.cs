#nullable enable
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Modules.Stocks.Commands;
using Asm.MooBank.Modules.Stocks.Tests.Support;
using DomainStockHolding = Asm.MooBank.Domain.Entities.StockHolding.StockHolding;

namespace Asm.MooBank.Modules.Stocks.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTests
{
    private readonly TestMocks _mocks;

    public UpdateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedStockHolding()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingHolding = TestEntities.CreateStockHolding(id: instrumentId, name: "Old Name");

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHolding);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
        };

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesEntityProperties()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingHolding = TestEntities.CreateStockHolding(
            id: instrumentId,
            name: "Old Name",
            description: "Old Description",
            currentPrice: 100m,
            shareWithFamily: false);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHolding);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New Name", existingHolding.Name);
        Assert.Equal("New Description", existingHolding.Description);
        Assert.True(existingHolding.ShareWithFamily);
        Assert.Equal(200m, existingHolding.CurrentPrice);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryUpdate()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHolding);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.StockHoldingRepositoryMock.Verify(r => r.Update(existingHolding), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var existingHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHolding);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentHolding_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainStockHolding)null!);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var existingHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<IncludeSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHolding);

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
            GroupId = groupId,
        };

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(groupId), Times.Once);
    }

    [Fact]
    public async Task Handle_NoGroupPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(groupId))
            .Throws(new NotAuthorisedException());

        var handler = new UpdateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Update
        {
            InstrumentId = instrumentId,
            Name = "New Name",
            Description = "New Description",
            ShareWithFamily = true,
            CurrentPrice = 200m,
            GroupId = groupId,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }
}
