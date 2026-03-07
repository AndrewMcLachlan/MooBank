using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.EventHandlers;
using Asm.MooBank.Models;
using Moq;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Domain.Tests.EventHandlers;

/// <summary>
/// Unit tests for the <see cref="AccountAddedEventHandler"/> class.
/// Tests verify that opening balance transactions are created correctly.
/// </summary>
public class AccountAddedEventHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _familyId = Guid.NewGuid();
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Models.User _user;
    private readonly AccountAddedEventHandler _handler;

    public AccountAddedEventHandlerTests()
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
        _handler = new AccountAddedEventHandler(_user, _mockRepository.Object);
    }

    #region Opening Balance Creation

    /// <summary>
    /// Given an AccountAddedEvent with a positive opening balance
    /// When Handle is called
    /// Then a transaction should be added to the repository
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithPositiveOpeningBalance_CreatesDomainTransaction()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 1000m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given an AccountAddedEvent with a negative opening balance
    /// When Handle is called
    /// Then a transaction should be added to the repository
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNegativeOpeningBalance_CreatesDomainTransaction()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = -500m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Once);
    }

    /// <summary>
    /// Given an AccountAddedEvent with zero opening balance
    /// When Handle is called
    /// Then no transaction should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithZeroOpeningBalance_DoesNotCreateDomainTransaction()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 0m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        _mockRepository.Verify(r => r.Add(It.IsAny<DomainTransaction>()), Times.Never);
    }

    #endregion

    #region DomainTransaction Properties

    /// <summary>
    /// Given an AccountAddedEvent
    /// When Handle is called
    /// Then the transaction should have correct amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesDomainTransactionWithCorrectAmount()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 2500m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);
        DomainTransaction? capturedDomainTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedDomainTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedDomainTransaction);
        Assert.Equal(openingBalance, capturedDomainTransaction.Amount);
    }

    /// <summary>
    /// Given an AccountAddedEvent
    /// When Handle is called
    /// Then the transaction should have "Opening Balance" description
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesDomainTransactionWithOpeningBalanceDescription()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 1000m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);
        DomainTransaction? capturedDomainTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedDomainTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedDomainTransaction);
        Assert.Equal("Opening Balance", capturedDomainTransaction.Description);
    }

    /// <summary>
    /// Given an AccountAddedEvent
    /// When Handle is called
    /// Then the transaction should have OpeningBalance sub-type
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesDomainTransactionWithOpeningBalanceSubType()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 1000m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);
        DomainTransaction? capturedDomainTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedDomainTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedDomainTransaction);
        Assert.Equal(TransactionSubType.OpeningBalance, capturedDomainTransaction.TransactionSubType);
    }

    /// <summary>
    /// Given an AccountAddedEvent with a specific opened date
    /// When Handle is called
    /// Then the transaction should have the correct transaction time
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesDomainTransactionWithCorrectDate()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 1000m;
        var openedDate = new DateOnly(2024, 6, 15);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);
        DomainTransaction? capturedDomainTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedDomainTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedDomainTransaction);
        Assert.Equal(openedDate.ToDateTime(TimeOnly.MinValue), capturedDomainTransaction.TransactionTime);
    }

    /// <summary>
    /// Given an AccountAddedEvent
    /// When Handle is called
    /// Then the transaction should have user's ID as account holder
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_CreatesDomainTransactionWithCorrectAccountHolder()
    {
        // Arrange
        var account = CreateLogicalAccount();
        var openingBalance = 1000m;
        var openedDate = DateOnly.FromDateTime(DateTime.Today);
        var domainEvent = new AccountAddedEvent(account, openingBalance, openedDate);
        DomainTransaction? capturedDomainTransaction = null;
        _mockRepository.Setup(r => r.Add(It.IsAny<DomainTransaction>()))
            .Callback<DomainTransaction>(t => capturedDomainTransaction = t);

        // Act
        await _handler.Handle(domainEvent, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(capturedDomainTransaction);
        Assert.Equal(_userId, capturedDomainTransaction.AccountHolderId);
    }

    #endregion

    private LogicalAccount CreateLogicalAccount()
    {
        var ownerId = Guid.NewGuid();
        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = ownerId }],
        };
    }
}
