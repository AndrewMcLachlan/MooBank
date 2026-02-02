#nullable enable
using Asm.MooBank.Modules.Stocks.Commands;
using Asm.MooBank.Modules.Stocks.Tests.Support;
using DomainStockHolding = Asm.MooBank.Domain.Entities.StockHolding.StockHolding;

namespace Asm.MooBank.Modules.Stocks.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedStockHolding()
    {
        // Arrange
        DomainStockHolding? capturedEntity = null;

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainStockHolding>()))
            .Callback<DomainStockHolding>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Apple Inc", result.Name);
        Assert.Equal("AAPL", result.Symbol);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        DomainStockHolding? capturedEntity = null;

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainStockHolding>()))
            .Callback<DomainStockHolding>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.StockHoldingRepositoryMock.Verify(r => r.Add(It.IsAny<DomainStockHolding>()), Times.Once);
        Assert.NotNull(capturedEntity);
        Assert.Equal("Apple Inc", capturedEntity.Name);
        Assert.Equal("Tech stock", capturedEntity.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesInitialTransaction()
    {
        // Arrange
        DomainStockHolding? capturedEntity = null;

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainStockHolding>()))
            .Callback<DomainStockHolding>(e => capturedEntity = e);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEntity);
        Assert.Single(capturedEntity.Transactions);
        var transaction = capturedEntity.Transactions.First();
        Assert.Equal(10, transaction.Quantity);
        Assert.Equal(150m, transaction.Price);
        Assert.Equal(9.95m, transaction.Fees);
    }

    [Fact]
    public async Task Handle_WithGroupId_ChecksGroupPermission()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
            GroupId = groupId,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(groupId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutGroupId_DoesNotCheckGroupPermission()
    {
        // Arrange
        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertGroupPermission(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoGroupPermission_ThrowsNotAuthorisedException()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        _mocks.SecurityMock
            .Setup(s => s.AssertGroupPermission(groupId))
            .Throws(new NotAuthorisedException());

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object,
            _mocks.User,
            _mocks.SecurityMock.Object,
            _mocks.CurrencyConverterMock.Object);

        var command = new Create
        {
            Name = "Apple Inc",
            Description = "Tech stock",
            Symbol = "AAPL",
            Price = 150m,
            Quantity = 10,
            Fees = 9.95m,
            ShareWithFamily = false,
            GroupId = groupId,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotAuthorisedException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }
}
