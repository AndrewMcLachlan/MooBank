using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Events;
using Asm.MooBank.Models;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="Transaction"/> entity.
/// Tests verify factory methods, type determination, and property calculations.
/// </summary>
public class TransactionTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    #region Transaction.Create - Type Determination

    /// <summary>
    /// Given a negative amount
    /// When Transaction.Create is called
    /// Then the transaction type should be Debit
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithNegativeAmount_SetsTypeToDebit()
    {
        // Arrange
        var amount = -50m;

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Equal(TransactionType.Debit, transaction.TransactionType);
    }

    /// <summary>
    /// Given a positive amount
    /// When Transaction.Create is called
    /// Then the transaction type should be Credit
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithPositiveAmount_SetsTypeToCredit()
    {
        // Arrange
        var amount = 100m;

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
    }

    /// <summary>
    /// Given a zero amount
    /// When Transaction.Create is called
    /// Then the transaction type should be Credit (not negative)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithZeroAmount_SetsTypeToCredit()
    {
        // Arrange
        var amount = 0m;

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
    }

    /// <summary>
    /// Given an explicit transaction type override
    /// When Transaction.Create is called with transactionType parameter
    /// Then the specified type should be used
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithExplicitType_UsesSpecifiedType()
    {
        // Arrange
        var amount = -50m; // Would normally be Debit

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null,
            TransactionType.Credit); // Override to Credit

        // Assert
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
    }

    #endregion

    #region Transaction.Create - Property Assignment

    /// <summary>
    /// Given transaction parameters
    /// When Transaction.Create is called
    /// Then all properties should be correctly assigned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_AssignsAllProperties()
    {
        // Arrange
        var amount = -75.50m;
        var description = "Test Transaction";
        var transactionTime = new DateTime(2024, 6, 15, 10, 30, 0);
        var subType = TransactionSubType.OpeningBalance;
        var source = "Import";
        var institutionAccountId = Guid.NewGuid();

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            description,
            transactionTime,
            subType,
            source,
            institutionAccountId);

        // Assert
        Assert.Equal(AccountId, transaction.AccountId);
        Assert.Equal(UserId, transaction.AccountHolderId);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(description, transaction.Description);
        Assert.Equal(transactionTime, transaction.TransactionTime);
        Assert.Equal(subType, transaction.TransactionSubType);
        Assert.Equal(source, transaction.Source);
        Assert.Equal(institutionAccountId, transaction.InstitutionAccountId);
    }

    /// <summary>
    /// Given null description
    /// When Transaction.Create is called
    /// Then description should be null
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithNullDescription_AllowsNull()
    {
        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            null,
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Null(transaction.Description);
    }

    /// <summary>
    /// Given null account holder ID
    /// When Transaction.Create is called
    /// Then account holder ID should be null
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithNullAccountHolder_AllowsNull()
    {
        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            null,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Null(transaction.AccountHolderId);
    }

    #endregion

    #region Transaction.Create - Events

    /// <summary>
    /// Given valid parameters
    /// When Transaction.Create is called
    /// Then a TransactionAddedEvent should be raised
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_RaisesTransactionAddedEvent()
    {
        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Single(transaction.Events);
        Assert.IsType<TransactionAddedEvent>(transaction.Events.First());
    }

    /// <summary>
    /// Given valid parameters
    /// When Transaction.Create is called
    /// Then the event should contain the created transaction
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_EventContainsTransaction()
    {
        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        var addedEvent = transaction.Events.First() as TransactionAddedEvent;
        Assert.NotNull(addedEvent);
        Assert.Same(transaction, addedEvent.Transaction);
    }

    #endregion

    #region Transaction.Create - Minimum Split

    /// <summary>
    /// Given valid parameters
    /// When Transaction.Create is called
    /// Then at least one split should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_CreatesMinimumSplit()
    {
        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Single(transaction.Splits);
    }

    /// <summary>
    /// Given a transaction amount
    /// When Transaction.Create is called
    /// Then the split amount should be the absolute value of the transaction amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_SplitHasAbsoluteAmount()
    {
        // Arrange
        var amount = -75.50m;

        // Act
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            amount,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Assert
        Assert.Equal(75.50m, transaction.Splits.First().Amount);
    }

    #endregion

    #region Transaction.NetAmount

    /// <summary>
    /// Given a debit transaction with no offsets
    /// When NetAmount is calculated
    /// Then it should return the negative of the split amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_DebitNoOffsets_ReturnsNegativeSplitAmount()
    {
        // Arrange
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -100m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(-100m, netAmount);
    }

    /// <summary>
    /// Given a credit transaction with no offsets
    /// When NetAmount is calculated
    /// Then it should return the split amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_CreditNoOffsets_ReturnsSplitAmount()
    {
        // Arrange
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            100m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(100m, netAmount);
    }

    #endregion

    #region Transaction.UpdateProperties

    /// <summary>
    /// Given a transaction
    /// When UpdateProperties is called
    /// Then Notes and ExcludeFromReporting should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateProperties_UpdatesNotesAndExcludeFromReporting()
    {
        // Arrange
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Act
        transaction.UpdateProperties("New notes", true);

        // Assert
        Assert.Equal("New notes", transaction.Notes);
        Assert.True(transaction.ExcludeFromReporting);
    }

    /// <summary>
    /// Given a transaction with existing notes
    /// When UpdateProperties is called with null notes
    /// Then Notes should be set to null
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateProperties_WithNullNotes_ClearsNotes()
    {
        // Arrange
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);
        transaction.UpdateProperties("Original notes", false);

        // Act
        transaction.UpdateProperties(null, false);

        // Assert
        Assert.Null(transaction.Notes);
    }

    #endregion
}
