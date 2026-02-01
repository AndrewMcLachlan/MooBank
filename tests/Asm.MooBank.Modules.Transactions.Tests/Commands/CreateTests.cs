#nullable enable
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class CreateTests
{
    private readonly TestMocks _mocks;

    public CreateTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsTransaction()
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

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -50m, "Test purchase", "REF001", DateTimeOffset.Now);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(-50m, result.Amount);
        Assert.Equal("Test purchase", result.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsToRepository()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -100m, "Groceries", null, DateTimeOffset.Now);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.TransactionRepositoryMock.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
        Assert.NotNull(capturedTransaction);
        Assert.Equal(-100m, capturedTransaction.Amount);
        Assert.Equal("Groceries", capturedTransaction.Description);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Returns<DomainTransaction>(t => t);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -50m, "Test", null, DateTimeOffset.Now);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsAccountHolderFromUserIdProvider()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mocks.UserIdProviderMock.Setup(u => u.CurrentUserId).Returns(userId);

        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -50m, "Test", null, DateTimeOffset.Now);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(userId, capturedTransaction.AccountHolderId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsReference()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -50m, "Test", "REF123", DateTimeOffset.Now);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("REF123", capturedTransaction.Reference);
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

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, -50m, "Test", null, DateTimeOffset.Now);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-50.50)]
    [InlineData(100)]
    [InlineData(0.01)]
    public async Task Handle_DifferentAmounts_SetsCorrectTransactionType(decimal amount)
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        DomainTransaction? capturedTransaction = null;
        _mocks.TransactionRepositoryMock
            .Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t)
            .Returns<DomainTransaction>(t => t);

        var handler = new CreateHandler(
            _mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UserIdProviderMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var command = new Create(instrumentId, amount, "Test", null, DateTimeOffset.Now);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        var expectedType = amount < 0 ? MooBank.Models.TransactionType.Debit : MooBank.Models.TransactionType.Credit;
        Assert.Equal(expectedType, capturedTransaction.TransactionType);
    }
}
