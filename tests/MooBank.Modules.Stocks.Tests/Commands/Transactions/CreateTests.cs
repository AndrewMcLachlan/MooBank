#nullable enable
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Commands.Transactions;
using Asm.MooBank.Modules.Stocks.Tests.Support;

namespace Asm.MooBank.Modules.Stocks.Tests.Commands.Transactions;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCreatedTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
        Assert.Equal(150m, result.Price);
        Assert.Equal(9.95m, result.Fees);
        Assert.Equal("Purchase shares", result.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsTransactionToStockHolding()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(stockHolding.Transactions);
        var transaction = stockHolding.Transactions.First();
        Assert.Equal(5, transaction.Quantity);
        Assert.Equal(150m, transaction.Price);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PositiveQuantity_SetsCreditTransactionType()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(TransactionType.Credit, result.TransactionType);
    }

    [Fact]
    public async Task Handle_NegativeQuantity_SetsDebitTransactionType()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: -3,
            Price: 150m,
            Fees: 9.95m,
            Description: "Sell shares",
            Date: DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(TransactionType.Debit, result.TransactionType);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAccountId()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(instrumentId, result.AccountId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsTransactionDate()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);
        var transactionDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: transactionDate);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(transactionDate, result.TransactionDate.DateTime);
    }

    [Fact]
    public async Task Handle_StockHoldingNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 5,
            Price: 150m,
            Fees: 9.95m,
            Description: "Purchase shares",
            Date: DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_ZeroQuantity_SetsDebitTransactionType()
    {
        // Arrange - Quantity=0 is edge case where it becomes Debit (0 > 0 is false)
        var instrumentId = Guid.NewGuid();
        var stockHolding = TestEntities.CreateStockHolding(id: instrumentId);

        _mocks.StockHoldingRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockHolding);

        var handler = new CreateHandler(
            _mocks.StockHoldingRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(
            InstrumentId: instrumentId,
            Quantity: 0,
            Price: 150m,
            Fees: 0m,
            Description: "Zero quantity edge case",
            Date: DateTime.UtcNow);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert - 0 > 0 is false, so TransactionType should be Debit
        Assert.Equal(TransactionType.Debit, result.TransactionType);
        Assert.Equal(0, result.Quantity);
    }
}
