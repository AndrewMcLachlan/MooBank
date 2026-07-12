using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Events;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Models;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="DomainTransaction"/> entity.
/// Tests verify factory methods, type determination, split management, and property calculations.
/// </summary>
public class TransactionTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly TestEntities _entities = new();

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

    /// <summary>
    /// Given various negative amounts
    /// When Transaction.Create is called
    /// Then TransactionType should be Debit
    /// </summary>
    [Theory]
    [InlineData(-50.00)]
    [InlineData(-0.01)]
    [InlineData(-1000.00)]
    [Trait("Category", "Unit")]
    public void Create_WithVariousNegativeAmounts_SetsTransactionTypeToDebit(decimal amount)
    {
        // Act
        var transaction = _entities.CreateTransaction(amount);

        // Assert
        Assert.Equal(TransactionType.Debit, transaction.TransactionType);
    }

    /// <summary>
    /// Given various positive amounts
    /// When Transaction.Create is called
    /// Then TransactionType should be Credit
    /// </summary>
    [Theory]
    [InlineData(50.00)]
    [InlineData(0.01)]
    [InlineData(1000.00)]
    [Trait("Category", "Unit")]
    public void Create_WithVariousPositiveAmounts_SetsTransactionTypeToCredit(decimal amount)
    {
        // Act
        var transaction = _entities.CreateTransaction(amount);

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

    /// <summary>
    /// Given any amount
    /// When Transaction.Create is called
    /// Then the transaction should have exactly one split with the absolute amount
    /// </summary>
    [Theory]
    [InlineData(-75.50, 75.50)]
    [InlineData(100.00, 100.00)]
    [Trait("Category", "Unit")]
    public void Create_Always_CreatesOneSplitWithAbsoluteAmount(decimal amount, decimal expectedSplitAmount)
    {
        // Act
        var transaction = _entities.CreateTransaction(amount);

        // Assert
        Assert.Single(transaction.Splits);
        Assert.Equal(expectedSplitAmount, transaction.Splits.First().Amount);
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

    /// <summary>
    /// Given a debit transaction with a split offset
    /// When NetAmount is accessed
    /// Then it should return the negated (amount minus offset)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_DebitWithSplitOffset_ReturnsNegatedAmountMinusOffset()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var split = transaction.Splits.First();
        split.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 25m,
        });

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(-75m, netAmount);
    }

    /// <summary>
    /// Given a debit transaction with multiple splits and offsets
    /// When NetAmount is calculated
    /// Then it should sum all splits and subtract all offsets correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_MultipleSplitsWithOffsets_CalculatesCorrectly()
    {
        // Arrange - Create transaction with multiple splits
        var transaction = _entities.CreateDebitTransaction(100m);
        var firstSplit = transaction.Splits.First();
        firstSplit.Amount = 60m;
        firstSplit.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = firstSplit.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 10m,
        });

        // Add a second split using UpdateSplits
        var updatedSplits = new List<Models.TransactionSplit>
        {
            new() { Id = firstSplit.Id, Amount = 60m, Tags = [], OffsetBy = [new TransactionOffsetBy { Amount = 10m, Transaction = new Models.Transaction { Id = Guid.NewGuid() } }] },
            new() { Id = Guid.NewGuid(), Amount = 40m, Tags = [], OffsetBy = [new TransactionOffsetBy { Amount = 5m, Transaction = new Models.Transaction { Id = Guid.NewGuid() } }] },
        };
        transaction.UpdateSplits(updatedSplits);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        // Split 1: 60 - 10 = 50
        // Split 2: 40 - 5 = 35
        // Total: 50 + 35 = 85
        // Debit negates: -85
        Assert.Equal(-85m, netAmount);
    }

    /// <summary>
    /// Given a debit transaction with an offset that equals the split amount
    /// When NetAmount is calculated
    /// Then it should return zero for that split's contribution
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_OffsetEqualsSplitAmount_ReturnsZeroContribution()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var split = transaction.Splits.First();
        split.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 100m, // Equals split amount
        });

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(0m, netAmount);
    }

    /// <summary>
    /// Given a transaction with very small decimal amounts
    /// When NetAmount is calculated
    /// Then precision should be maintained
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_VerySmallDecimals_MaintainsPrecision()
    {
        // Arrange
        var transaction = DomainTransaction.Create(
            AccountId,
            UserId,
            -0.0001m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(-0.0001m, netAmount);
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

    #region UpdateSplits

    /// <summary>
    /// Given a transaction with one split
    /// When UpdateSplits is called with an additional split
    /// Then the transaction should have two splits
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_AddingNewSplit_IncreasesSpitCount()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var existingSplit = transaction.Splits.First();
        var updatedSplits = new List<Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 70m, Tags = [], OffsetBy = [] },
            new() { Id = Guid.NewGuid(), Amount = 30m, Tags = [], OffsetBy = [] },
        };

        // Act
        transaction.UpdateSplits(updatedSplits);

        // Assert
        Assert.Equal(2, transaction.Splits.Count);
    }

    /// <summary>
    /// Given a transaction with one split
    /// When UpdateSplits is called with an empty list
    /// Then the transaction should retain the minimum of one split
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_WithEmptyList_RetainsMinimumOneSplit()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);

        // Act
        transaction.UpdateSplits([]);

        // Assert
        Assert.Single(transaction.Splits);
    }

    /// <summary>
    /// Given a transaction with a split of amount 50
    /// When UpdateSplits is called with the same split ID but amount 75
    /// Then the split amount should be updated to 75
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_WithUpdatedAmount_UpdatesExistingSplitAmount()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var existingSplitId = transaction.Splits.First().Id;
        var updatedSplits = new List<Models.TransactionSplit>
        {
            new() { Id = existingSplitId, Amount = 75m, Tags = [], OffsetBy = [] },
        };

        // Act
        transaction.UpdateSplits(updatedSplits);

        // Assert
        Assert.Equal(75m, transaction.Splits.First().Amount);
    }

    /// <summary>
    /// Given a transaction with two splits
    /// When UpdateSplits removes one split
    /// Then only one split should remain
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_RemovingSplit_DecreasesSplitCount()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var existingSplit = transaction.Splits.First();

        // Add a second split first
        var splitId2 = Guid.NewGuid();
        var splitsWithTwo = new List<Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 70m, Tags = [], OffsetBy = [] },
            new() { Id = splitId2, Amount = 30m, Tags = [], OffsetBy = [] },
        };
        transaction.UpdateSplits(splitsWithTwo);
        Assert.Equal(2, transaction.Splits.Count);

        // Now remove one
        var splitsWithOne = new List<Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 100m, Tags = [], OffsetBy = [] },
        };

        // Act
        transaction.UpdateSplits(splitsWithOne);

        // Assert
        Assert.Single(transaction.Splits);
    }

    #endregion

    #region UpdateSplits with Offsets

    /// <summary>
    /// Given a transaction split with an offset
    /// When UpdateSplits is called with an updated offset amount
    /// Then the offset amount should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_WithUpdatedOffset_UpdatesOffsetAmount()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var existingSplit = transaction.Splits.First();
        var offsetTransactionId = Guid.NewGuid();

        // Add an initial offset
        existingSplit.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = existingSplit.Id,
            OffsetTransactionId = offsetTransactionId,
            Amount = 25m,
        });

        var updatedSplits = new List<Models.TransactionSplit>
        {
            new()
            {
                Id = existingSplit.Id,
                Amount = 100m,
                Tags = [],
                OffsetBy = [new TransactionOffsetBy { Amount = 50m, Transaction = new Models.Transaction { Id = offsetTransactionId } }]
            },
        };

        // Act
        transaction.UpdateSplits(updatedSplits);

        // Assert
        var offset = transaction.Splits.First().OffsetBy.First();
        Assert.Equal(50m, offset.Amount);
    }

    /// <summary>
    /// Given a transaction split with an offset
    /// When UpdateSplits is called without that offset
    /// Then the offset should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateSplits_WithRemovedOffset_RemovesOffset()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var existingSplit = transaction.Splits.First();

        existingSplit.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = existingSplit.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 25m,
        });

        var updatedSplits = new List<Models.TransactionSplit>
        {
            new()
            {
                Id = existingSplit.Id,
                Amount = 100m,
                Tags = [],
                OffsetBy = [] // No offsets
            },
        };

        // Act
        transaction.UpdateSplits(updatedSplits);

        // Assert
        Assert.Empty(transaction.Splits.First().OffsetBy);
    }

    #endregion

    #region AddOrUpdateSplit

    /// <summary>
    /// Given a transaction with no tags on its split
    /// When AddOrUpdateSplit is called with a tag
    /// Then the split should contain the tag
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddOrUpdateSplit_WithNewTag_AddsTagToFirstSplit()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var tag = _entities.CreateTag(1, "Groceries");

        // Act
        transaction.AddOrUpdateSplit(tag);

        // Assert
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
    }

    /// <summary>
    /// Given a transaction with an existing tag on its split
    /// When AddOrUpdateSplit is called with a different tag
    /// Then both tags should be present
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddOrUpdateSplit_WithMultipleTags_AddsBothTags()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Food");

        // Act
        transaction.AddOrUpdateSplit(tag1);
        transaction.AddOrUpdateSplit(tag2);

        // Assert
        Assert.Equal(2, transaction.Tags.Count());
    }

    /// <summary>
    /// Given a transaction with existing tags
    /// When AddOrUpdateSplit is called with the same tag
    /// Then no duplicates should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddOrUpdateSplit_WithDuplicateTag_DoesNotAddDuplicate()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var tag = _entities.CreateTag(1, "Groceries");

        // Act
        transaction.AddOrUpdateSplit(tag);
        transaction.AddOrUpdateSplit(tag); // Add same tag again

        // Assert
        Assert.Single(transaction.Tags);
    }

    /// <summary>
    /// Given a manually created transaction with no splits
    /// When AddOrUpdateSplit is called with tags
    /// Then a new split should be created with those tags
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddOrUpdateSplit_WhenNoSplitExists_CreatesNewSplit()
    {
        // Arrange - Create a transaction manually without factory (no splits)
        var transaction = new DomainTransaction
        {
            Amount = 50m,
            Source = "Test",
        };
        var tag = _entities.CreateTag(1, "Test");

        // Act
        transaction.AddOrUpdateSplit([tag]);

        // Assert
        Assert.Single(transaction.Splits);
        Assert.Contains(transaction.Tags, t => t.Id == tag.Id);
    }

    #endregion

    #region UpdateOrRemoveSplit

    /// <summary>
    /// Given a transaction with a tag on its split
    /// When UpdateOrRemoveSplit is called with that tag
    /// Then the tag should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateOrRemoveSplit_WithExistingTag_RemovesTag()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Food");
        transaction.AddOrUpdateSplit([tag1, tag2]);

        // Act
        transaction.UpdateOrRemoveSplit(tag1);

        // Assert
        Assert.Single(transaction.Tags);
        Assert.DoesNotContain(transaction.Tags, t => t.Id == tag1.Id);
    }

    /// <summary>
    /// Given a transaction with only one tag
    /// When UpdateOrRemoveSplit is called with that tag
    /// Then the split should remain but be empty (minimum one split rule)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateOrRemoveSplit_LastTag_KeepsSplitButRemovesTag()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);
        var tag = _entities.CreateTag(1, "Groceries");
        transaction.AddOrUpdateSplit(tag);

        // Act
        transaction.UpdateOrRemoveSplit(tag);

        // Assert
        Assert.Single(transaction.Splits);
        Assert.Empty(transaction.Tags);
    }

    #endregion

    #region EnsureMinimumSplit

    /// <summary>
    /// Given a transaction
    /// When created via factory
    /// Then it should always have at least one split
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EnsureMinimumSplit_Always_HasAtLeastOneSplit()
    {
        // Arrange & Act
        var transaction = _entities.CreateDebitTransaction(100m);

        // Assert
        Assert.NotEmpty(transaction.Splits);
    }

    #endregion

    #region Tags Property

    /// <summary>
    /// Given a transaction with tags across multiple splits
    /// When Tags property is accessed
    /// Then all tags from all splits should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Tags_WithMultipleSplits_ReturnsAllTags()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);
        var existingSplit = transaction.Splits.First();

        // Add another split with different tag
        var updatedSplits = new List<Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 50m, Tags = [new Tag { Id = 1, Name = "Tag1" }], OffsetBy = [] },
            new() { Id = Guid.NewGuid(), Amount = 50m, Tags = [new Tag { Id = 2, Name = "Tag2" }], OffsetBy = [] },
        };
        transaction.UpdateSplits(updatedSplits);

        // Act
        var tags = transaction.Tags.ToList();

        // Assert
        Assert.Equal(2, tags.Count);
    }

    #endregion
}
