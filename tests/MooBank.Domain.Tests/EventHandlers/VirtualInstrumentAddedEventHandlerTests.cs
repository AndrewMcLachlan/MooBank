using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.EventHandlers;
using Asm.MooBank.Models;
using Moq;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Domain.Tests.EventHandlers;

/// <summary>
/// Unit tests for the <see cref="VirtualInstrumentAddedEventHandler"/> class.
/// Tests verify that opening balance transactions are created for virtual instruments.
/// </summary>
public class VirtualInstrumentAddedEventHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Models.User _user;
    private readonly VirtualInstrumentAddedEventHandler _handler;

    public VirtualInstrumentAddedEventHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _user = new Models.User
        {
            Id = _userId,
            EmailAddress = "test@test.com",
            FamilyId = _familyId,
            Currency = "AUD",
            Accounts = [],
            SharedAccounts = [],
        };
        _handler = new VirtualInstrumentAddedEventHandler(_user, _mockRepository.Object);
    }

    #region Opening Balance Creation

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent with a positive opening balance
    /// When Handle is called
    /// Then a transaction should be added to the repository
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPositiveOpeningBalance_CreatesTransaction()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 500m);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent with a negative opening balance
    /// When Handle is called
    /// Then a transaction should be added to the repository
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNegativeOpeningBalance_CreatesTransaction()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, -200m);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent with zero opening balance
    /// When Handle is called
    /// Then no transaction should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithZeroOpeningBalance_DoesNotCreateTransaction()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 0m);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
    }

    #endregion

    #region Transaction Properties

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent
    /// When Handle is called
    /// Then the transaction should have correct amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithCorrectAmount()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var openingBalance = 1500m;
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, openingBalance);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(openingBalance, capturedTransaction.Amount);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent
    /// When Handle is called
    /// Then the transaction should have "Opening Balance" description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithOpeningBalanceDescription()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 100m);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("Opening Balance", capturedTransaction.Description);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent
    /// When Handle is called
    /// Then the transaction should have OpeningBalance sub-type
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithOpeningBalanceSubType()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 100m);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(TransactionSubType.OpeningBalance, capturedTransaction.TransactionSubType);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent
    /// When Handle is called
    /// Then the transaction should have "Event" as source
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithEventSource()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 100m);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal("Event", capturedTransaction.Source);
    }

    /// <summary>
    /// Given a VirtualInstrumentAddedEvent
    /// When Handle is called
    /// Then the transaction should have user's ID as account holder
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesTransactionWithCorrectAccountHolder()
    {
        // Arrange
        var virtualInstrument = CreateVirtualInstrument();
        var domainEvent = new VirtualInstrumentAddedEvent(virtualInstrument, 100m);
        DomainTransaction? capturedTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(_userId, capturedTransaction.AccountHolderId);
    }

    #endregion

    private DomainVirtualInstrument CreateVirtualInstrument()
    {
        return new DomainVirtualInstrument(Guid.NewGuid())
        {
            Name = "Savings Goal",
            Currency = "AUD",
            ParentInstrumentId = Guid.NewGuid(),
        };
    }
}
