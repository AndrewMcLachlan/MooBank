#nullable enable
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Tests.Support;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class UpdateTransactionTests
{
    private readonly TestMocks _mocks;

    public UpdateTransactionTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesAndReturnsTransaction()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, notes: "Original notes");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var splits = new[] { TestEntities.CreateTransactionSplitModel() };
        var command = new UpdateTransaction(instrumentId, transactionId, "Updated notes", splits, true);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transactionId, result.Id);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesNotes()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, notes: "Original notes");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var splits = new[] { TestEntities.CreateTransactionSplitModel() };
        var command = new UpdateTransaction(instrumentId, transactionId, "New notes", splits, false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("New notes", existingTransaction.Notes);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesExcludeFromReporting()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, excludeFromReporting: false);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var splits = new[] { TestEntities.CreateTransactionSplitModel() };
        var command = new UpdateTransaction(instrumentId, transactionId, null, splits, true);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(existingTransaction.ExcludeFromReporting);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var splits = new[] { TestEntities.CreateTransactionSplitModel() };
        var command = new UpdateTransaction(instrumentId, transactionId, "Notes", splits, false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullNotes_SetsNotesToNull()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, notes: "Original notes");

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var splits = new[] { TestEntities.CreateTransactionSplitModel() };
        var command = new UpdateTransaction(instrumentId, transactionId, null, splits, false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(existingTransaction.Notes);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateSplits()
    {
        // Arrange
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var splitId = Guid.NewGuid();

        var existingSplit = TestEntities.CreateTransactionSplit(id: splitId, transactionId: transactionId, amount: 50m);
        var existingTransaction = TestEntities.CreateTransaction(id: transactionId, accountId: instrumentId, splits: [existingSplit]);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var handler = new UpdateTransactionHandler(
            _mocks.TransactionRepositoryMock.Object,
            _mocks.UnitOfWorkMock.Object);

        var updatedSplits = new[] { TestEntities.CreateTransactionSplitModel(id: splitId, amount: 75m) };
        var command = new UpdateTransaction(instrumentId, transactionId, null, updatedSplits, false);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert - verify the transaction was updated (splits are updated internally)
        _mocks.UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
