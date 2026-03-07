#nullable enable
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Models;
using Asm.MooBank.Modules.Transactions.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateBalanceTests
{
    private readonly TestMocks _mocks;

    public UpdateBalanceTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesBalanceAdjustmentTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, "Balance update", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedTransaction);
        Assert.Equal(500m, capturedTransaction.Amount); // 1500 - 1000 = 500
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, "Balance update", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BalanceDecrease_CreatesNegativeAmount()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(500m, "Balance decrease", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(-500m, capturedTransaction.Amount); // 500 - 1000 = -500
    }

    [Fact]
    public async Task Handle_NoDescription_UsesDefaultDescription()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, null!, null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("Balance adjustment", capturedTransaction.Description);
    }

    [Fact]
    public async Task Handle_WithDescription_UsesProvidedDescription()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, "Custom description", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("Custom description", capturedTransaction.Description);
    }

    [Fact]
    public async Task Handle_NonTransactionInstrument_ThrowsInvalidOperationException()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var nonTransactionInstrument = new Mock<Instrument>(instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nonTransactionInstrument.Object);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1500m, "Test", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_SameBalance_CreatesZeroAmountTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId, balance: 1000m);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new UpdateBalanceHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var balanceUpdate = new CreateTransaction(1000m, "No change", null, DateTimeOffset.Now);
        var command = new UpdateBalance(instrumentId, balanceUpdate);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(0m, capturedTransaction.Amount);
    }
}
