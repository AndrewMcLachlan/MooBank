#nullable enable
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Commands;
using Asm.MooBank.Modules.Transactions.Tests.Support;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Transactions.Tests.Commands;

[Trait("Category", "Unit")]
public class DeleteTests
{
    private readonly TestMocks _mocks;

    public DeleteTests()
    {
        _mocks = new TestMocks();
    }

    private DeleteHandler CreateHandler() =>
        new(_mocks.InstrumentRepositoryMock.Object,
            _mocks.TransactionRepositoryMock.Object,
            _mocks.AuditingUnitOfWorkMock.Object);

    private (Guid InstrumentId, Guid TransactionId) Arrange(Controller controller, Guid? transactionAccountId = null)
    {
        var instrumentId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        var instrument = TestEntities.CreateTransactionInstrument(id: instrumentId);
        instrument.Controller = controller;

        _mocks.InstrumentRepositoryMock
            .Setup(r => r.Get(instrumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(instrument);

        var transaction = TestEntities.CreateTransaction(id: transactionId, accountId: transactionAccountId ?? instrumentId);

        _mocks.TransactionRepositoryMock
            .Setup(r => r.Get(transactionId, It.IsAny<IncludeSplitsAndOffsetsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        return (instrumentId, transactionId);
    }

    /// <summary>
    /// Given a transaction on a manual account
    /// When the delete command is handled
    /// Then the transaction should be deleted and the change saved
    /// </summary>
    [Fact]
    public async Task Handle_ManualAccount_DeletesTransaction()
    {
        // Arrange
        var (instrumentId, transactionId) = Arrange(Controller.Manual);

        // Act
        await CreateHandler().Handle(new Delete(instrumentId, transactionId), TestContext.Current.CancellationToken);

        // Assert
        _mocks.TransactionRepositoryMock.Verify(r => r.Delete(It.Is<DomainTransaction>(t => t.Id == transactionId)), Times.Once);
        _mocks.AuditingUnitOfWorkMock.Verify(u => u.SaveChangesAsync("Deleted", "Transaction", transactionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Given a transaction on a virtual account
    /// When the delete command is handled
    /// Then the transaction should be deleted
    /// </summary>
    [Fact]
    public async Task Handle_VirtualAccount_DeletesTransaction()
    {
        // Arrange
        var (instrumentId, transactionId) = Arrange(Controller.Virtual);

        // Act
        await CreateHandler().Handle(new Delete(instrumentId, transactionId), TestContext.Current.CancellationToken);

        // Assert
        _mocks.TransactionRepositoryMock.Verify(r => r.Delete(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given a transaction on an account whose transactions come from an importer
    /// When the delete command is handled
    /// Then it should be refused and nothing saved
    /// </summary>
    [Fact]
    public async Task Handle_ImportAccount_ThrowsAndDeletesNothing()
    {
        // Arrange
        var (instrumentId, transactionId) = Arrange(Controller.Import);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(new Delete(instrumentId, transactionId), TestContext.Current.CancellationToken).AsTask());

        _mocks.TransactionRepositoryMock.Verify(r => r.Delete(It.IsAny<DomainTransaction>()), Times.Never);
        _mocks.AuditingUnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a transaction belonging to a different instrument from the one in the route
    /// When the delete command is handled
    /// Then a NotFoundException should be thrown and nothing deleted
    /// </summary>
    [Fact]
    public async Task Handle_TransactionBelongsToDifferentInstrument_ThrowsNotFoundException()
    {
        // Arrange
        var (instrumentId, transactionId) = Arrange(Controller.Manual, transactionAccountId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(new Delete(instrumentId, transactionId), TestContext.Current.CancellationToken).AsTask());

        _mocks.TransactionRepositoryMock.Verify(r => r.Delete(It.IsAny<DomainTransaction>()), Times.Never);
    }
}
