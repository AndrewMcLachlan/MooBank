using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Transaction"/> domain entity.
/// Tests cover creation, type determination, split management, and net amount calculations.
/// </summary>
public class TransactionTests
{
    private readonly TestEntities _entities = new();

    #region Transaction.Create

    /// <summary>
    /// Given a negative amount
    /// When Transaction.Create is called
    /// Then TransactionType should be Debit
    /// </summary>
    [Theory]
    [InlineData(-50.00)]
    [InlineData(-0.01)]
    [InlineData(-1000.00)]
    [Trait("Category", "Unit")]
    public void Create_WithNegativeAmount_SetsTransactionTypeToDebit(decimal amount)
    {
        // Act
        var transaction = _entities.CreateTransaction(amount);

        // Assert
        Assert.Equal(TransactionType.Debit, transaction.TransactionType);
    }

    /// <summary>
    /// Given a positive amount
    /// When Transaction.Create is called
    /// Then TransactionType should be Credit
    /// </summary>
    [Theory]
    [InlineData(50.00)]
    [InlineData(0.01)]
    [InlineData(1000.00)]
    [Trait("Category", "Unit")]
    public void Create_WithPositiveAmount_SetsTransactionTypeToCredit(decimal amount)
    {
        // Act
        var transaction = _entities.CreateTransaction(amount);

        // Assert
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
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

    /// <summary>
    /// Given any amount
    /// When Transaction.Create is called
    /// Then a TransactionAddedEvent domain event should be raised
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_Always_RaisesTransactionAddedEvent()
    {
        // Act
        var transaction = _entities.CreateTransaction(-25.00m);

        // Assert
        Assert.Contains(transaction.Events, e => e.GetType().Name == "TransactionAddedEvent");
    }

    #endregion

    #region NetAmount

    /// <summary>
    /// Given a debit transaction with no offsets
    /// When NetAmount is accessed
    /// Then it should return the negated split amount (debits are negative)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_DebitWithNoOffsets_ReturnsNegatedSplitAmount()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(100m);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(-100m, netAmount);
    }

    /// <summary>
    /// Given a credit transaction with no offsets
    /// When NetAmount is accessed
    /// Then it should return the split amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void NetAmount_CreditWithNoOffsets_ReturnsSplitAmount()
    {
        // Arrange
        var transaction = _entities.CreateCreditTransaction(200m);

        // Act
        var netAmount = transaction.NetAmount;

        // Assert
        Assert.Equal(200m, netAmount);
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
        var updatedSplits = new List<MooBank.Models.TransactionSplit>
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
        var updatedSplits = new List<MooBank.Models.TransactionSplit>
        {
            new() { Id = existingSplitId, Amount = 75m, Tags = [], OffsetBy = [] },
        };

        // Act
        transaction.UpdateSplits(updatedSplits);

        // Assert
        Assert.Equal(75m, transaction.Splits.First().Amount);
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

    #region UpdateProperties

    /// <summary>
    /// Given a transaction with no notes
    /// When UpdateProperties is called with notes and excludeFromReporting true
    /// Then both properties should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateProperties_SetsNotesAndExcludeFromReporting()
    {
        // Arrange
        var transaction = _entities.CreateDebitTransaction(50m);

        // Act
        transaction.UpdateProperties("Test notes", true);

        // Assert
        Assert.Equal("Test notes", transaction.Notes);
        Assert.True(transaction.ExcludeFromReporting);
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

    #region Create with AccountId

    /// <summary>
    /// Given a negative amount and explicit transaction type of Credit
    /// When Transaction.Create with AccountId is called
    /// Then TransactionType should use the explicit type
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithExplicitTransactionType_UsesExplicitType()
    {
        // Act
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            -50m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            null,
            TransactionType.Credit);

        // Assert - Uses explicit type even though amount is negative
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
    }

    /// <summary>
    /// Given an amount with no explicit type
    /// When Transaction.Create with AccountId is called
    /// Then TransactionType should be determined by amount sign
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WithoutExplicitType_DeterminesFromAmount()
    {
        // Act
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            "Test",
            DateTime.UtcNow,
            null,
            "Test",
            Guid.NewGuid());

        // Assert
        Assert.Equal(TransactionType.Credit, transaction.TransactionType);
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

        var updatedSplits = new List<MooBank.Models.TransactionSplit>
        {
            new()
            {
                Id = existingSplit.Id,
                Amount = 100m,
                Tags = [],
                OffsetBy = [new MooBank.Models.TransactionOffsetBy { Amount = 50m, Transaction = new MooBank.Models.Transaction { Id = offsetTransactionId } }]
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

        var updatedSplits = new List<MooBank.Models.TransactionSplit>
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
        var splitsWithTwo = new List<MooBank.Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 70m, Tags = [], OffsetBy = [] },
            new() { Id = splitId2, Amount = 30m, Tags = [], OffsetBy = [] },
        };
        transaction.UpdateSplits(splitsWithTwo);
        Assert.Equal(2, transaction.Splits.Count);

        // Now remove one
        var splitsWithOne = new List<MooBank.Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 100m, Tags = [], OffsetBy = [] },
        };

        // Act
        transaction.UpdateSplits(splitsWithOne);

        // Assert
        Assert.Single(transaction.Splits);
    }

    #endregion

    #region AddOrUpdateSplit with no existing split

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
        var transaction = new Transaction
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
        var tag1 = _entities.CreateTag(1, "Tag1");
        var tag2 = _entities.CreateTag(2, "Tag2");

        // Add another split with different tag
        var updatedSplits = new List<MooBank.Models.TransactionSplit>
        {
            new() { Id = existingSplit.Id, Amount = 50m, Tags = [new MooBank.Models.Tag { Id = 1, Name = "Tag1" }], OffsetBy = [] },
            new() { Id = Guid.NewGuid(), Amount = 50m, Tags = [new MooBank.Models.Tag { Id = 2, Name = "Tag2" }], OffsetBy = [] },
        };
        transaction.UpdateSplits(updatedSplits);

        // Act
        var tags = transaction.Tags.ToList();

        // Assert
        Assert.Equal(2, tags.Count);
    }

    #endregion
}
