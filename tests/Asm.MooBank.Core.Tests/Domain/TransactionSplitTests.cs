using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="TransactionSplit"/> domain entity.
/// Tests cover tag management, offset handling, and net amount calculations.
/// </summary>
public class TransactionSplitTests
{
    private readonly TestEntities _entities = new();

    #region UpdateTags

    /// <summary>
    /// Given a TransactionSplit with no tags
    /// When UpdateTags is called with two tags
    /// Then the split should have two tags
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_AddingNewTags_AddsTagsToCollection()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
            Tags = [],
        };
        var tags = new[] { _entities.CreateTag(1, "Groceries"), _entities.CreateTag(2, "Food") };

        // Act
        split.UpdateTags(tags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
    }

    /// <summary>
    /// Given a TransactionSplit with tags "Groceries", "Food", "Shopping"
    /// When UpdateTags is called with only "Groceries"
    /// Then the split should have only one tag
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_RemovingTags_RemovesTagsNotInUpdate()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
            Tags = [
                _entities.CreateTag(1, "Groceries"),
                _entities.CreateTag(2, "Food"),
                _entities.CreateTag(3, "Shopping"),
            ],
        };

        // Act
        split.UpdateTags([_entities.CreateTag(1, "Groceries")]);

        // Assert
        Assert.Single(split.Tags);
        Assert.Equal("Groceries", split.Tags.First().Name);
    }

    #endregion

    #region GetNetAmount

    /// <summary>
    /// Given a TransactionSplit with amount 100 and no offsets
    /// When GetNetAmount is called
    /// Then it should return 100
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_WithNoOffsets_ReturnsFullAmount()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(100m, netAmount);
    }

    /// <summary>
    /// Given a TransactionSplit with amount 100 and an offset of 25
    /// When GetNetAmount is called
    /// Then it should return 75
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_WithOffset_ReturnsAmountMinusOffset()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
        };
        split.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 25m,
        });

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(75m, netAmount);
    }

    #endregion

    #region RemoveOffset

    /// <summary>
    /// Given a TransactionSplit with one offset
    /// When RemoveOffset is called with that offset
    /// Then the OffsetBy collection should be empty
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveOffset_WithExistingOffset_RemovesFromCollection()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
        };
        var offset = new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 25m,
        };
        split.OffsetBy.Add(offset);

        // Act
        split.RemoveOffset(offset);

        // Assert
        Assert.Empty(split.OffsetBy);
    }

    #endregion

    #region Multiple Offsets

    /// <summary>
    /// Given a TransactionSplit with amount 100 and multiple offsets totaling 40
    /// When GetNetAmount is called
    /// Then it should return 60
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_WithMultipleOffsets_ReturnsAmountMinusTotalOffsets()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
        };
        split.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 25m,
        });
        split.OffsetBy.Add(new TransactionOffset
        {
            TransactionSplitId = split.Id,
            OffsetTransactionId = Guid.NewGuid(),
            Amount = 15m,
        });

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(60m, netAmount);
    }

    #endregion

    #region UpdateTags Edge Cases

    /// <summary>
    /// Given a TransactionSplit with no tags
    /// When UpdateTags is called with empty collection
    /// Then Tags should remain empty
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_WithEmptyCollection_KeepsTagsEmpty()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
            Tags = [],
        };

        // Act
        split.UpdateTags([]);

        // Assert
        Assert.Empty(split.Tags);
    }

    /// <summary>
    /// Given a TransactionSplit with tags
    /// When UpdateTags is called with an empty collection
    /// Then all tags should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_WithEmptyCollectionFromTags_RemovesAllTags()
    {
        // Arrange
        var split = new TransactionSplit(Guid.NewGuid())
        {
            TransactionId = TestModels.TransactionId,
            Amount = 100m,
            Tags = [
                _entities.CreateTag(1, "Groceries"),
                _entities.CreateTag(2, "Food"),
            ],
        };

        // Act
        split.UpdateTags([]);

        // Assert
        Assert.Empty(split.Tags);
    }

    #endregion
}
