using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.EventHandlers;
using Asm.MooBank.Models;
using Moq;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Domain.Tests.EventHandlers;

/// <summary>
/// Unit tests for the <see cref="BalanceAdjustmentEventHandler"/> class.
/// Tests verify that balance adjustment transactions are created correctly.
/// </summary>
public class BalanceAdjustmentEventHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly BalanceAdjustmentEventHandler _handler;

    public BalanceAdjustmentEventHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _handler = new BalanceAdjustmentEventHandler(_mockRepository.Object);
    }

    #region Transaction Creation

    /// <summary>
    /// Given a BalanceAdjustmentEvent with a TransactionInstrument
    /// When Handle is called
    /// Then a transaction should be added to the repository
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithTransactionInstrument_CreatesTransaction()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, "Test");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent with a non-TransactionInstrument
    /// When Handle is called
    /// Then no transaction should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonTransactionInstrument_DoesNotCreateTransaction()
    {
        // Arrange
        var instrument = CreateStockHolding();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, "Test");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent with positive amount
    /// When Handle is called
    /// Then a credit transaction should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPositiveAmount_CreatesTransaction()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 500m, "Deposit");
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(500m, capturedTransaction.Amount);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent with negative amount
    /// When Handle is called
    /// Then a debit transaction should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNegativeAmount_CreatesTransaction()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, -200m, "Withdrawal");
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(-200m, capturedTransaction.Amount);
    }

    #endregion

    #region Transaction Properties

    /// <summary>
    /// Given a BalanceAdjustmentEvent
    /// When Handle is called
    /// Then the transaction should have "Balance adjustment" description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithBalanceAdjustmentDescription()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, "Test");
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("Balance adjustment", capturedTransaction.Description);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent
    /// When Handle is called
    /// Then the transaction should have BalanceAdjustment sub-type
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithBalanceAdjustmentSubType()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, "Test");
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(TransactionSubType.BalanceAdjustment, capturedTransaction.TransactionSubType);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent with a source
    /// When Handle is called
    /// Then the transaction should have the correct source
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithCorrectSource()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var source = "ImportProcess";
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, source);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(source, capturedTransaction.Source);
    }

    /// <summary>
    /// Given a BalanceAdjustmentEvent
    /// When Handle is called
    /// Then the transaction should have null account holder (system-generated)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithNullAccountHolder()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 100m, "Test");
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Null(capturedTransaction.AccountHolderId);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Given a BalanceAdjustmentEvent with zero amount
    /// When Handle is called
    /// Then a transaction should still be created (no zero check)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithZeroAmount_StillCreatesTransaction()
    {
        // Arrange
        var instrument = CreateLogicalAccount();
        var domainEvent = new BalanceAdjustmentEvent(instrument, 0m, "Test");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    #endregion

    private LogicalAccount CreateLogicalAccount()
    {
        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = Guid.NewGuid() }],
        };
    }

    private StockHolding CreateStockHolding()
    {
        return new StockHolding(Guid.NewGuid())
        {
            Name = "Test Stock",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = Guid.NewGuid() }],
        };
    }
}
