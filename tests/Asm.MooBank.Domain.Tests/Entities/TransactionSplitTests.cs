using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="TransactionSplit"/> entity.
/// Tests verify tag management and net amount calculations.
/// </summary>
public class TransactionSplitTests
{
    private static readonly Guid FamilyId = Guid.NewGuid();

    #region GetNetAmount

    /// <summary>
    /// Given a split with no offsets
    /// When GetNetAmount is called
    /// Then it should return the full amount
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_NoOffsets_ReturnsFullAmount()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy = []
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(100m, netAmount);
    }

    /// <summary>
    /// Given a split with one offset
    /// When GetNetAmount is called
    /// Then it should return amount minus offset
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_WithOneOffset_SubtractsOffset()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy =
            [
                new TransactionOffset { Amount = 25m }
            ]
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(75m, netAmount);
    }

    /// <summary>
    /// Given a split with multiple offsets
    /// When GetNetAmount is called
    /// Then it should return amount minus sum of all offsets
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_WithMultipleOffsets_SubtractsAllOffsets()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy =
            [
                new TransactionOffset { Amount = 20m },
                new TransactionOffset { Amount = 30m },
                new TransactionOffset { Amount = 10m }
            ]
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(40m, netAmount); // 100 - 20 - 30 - 10
    }

    /// <summary>
    /// Given a split with offsets totaling the full amount
    /// When GetNetAmount is called
    /// Then it should return zero
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_OffsetsEqualAmount_ReturnsZero()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy =
            [
                new TransactionOffset { Amount = 100m }
            ]
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(0m, netAmount);
    }

    /// <summary>
    /// Given a split with offsets exceeding the amount
    /// When GetNetAmount is called
    /// Then it should return a negative value
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_OffsetsExceedAmount_ReturnsNegative()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 50m,
            OffsetBy =
            [
                new TransactionOffset { Amount = 75m }
            ]
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(-25m, netAmount);
    }

    /// <summary>
    /// Given a split with zero amount and no offsets
    /// When GetNetAmount is called
    /// Then it should return zero
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetNetAmount_ZeroAmount_ReturnsZero()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Amount = 0m,
            OffsetBy = []
        };

        // Act
        var netAmount = split.GetNetAmount();

        // Assert
        Assert.Equal(0m, netAmount);
    }

    #endregion

    #region UpdateTags - Adding Tags

    /// <summary>
    /// Given a split with no tags
    /// When UpdateTags is called with new tags
    /// Then those tags should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_EmptySplit_AddsNewTags()
    {
        // Arrange
        var split = new TransactionSplit { Tags = [] };
        var newTags = new List<Tag>
        {
            CreateTag(1, "Groceries"),
            CreateTag(2, "Food"),
        };

        // Act
        split.UpdateTags(newTags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
        Assert.Contains(split.Tags, t => t.Id == 1);
        Assert.Contains(split.Tags, t => t.Id == 2);
    }

    /// <summary>
    /// Given a split with existing tags
    /// When UpdateTags is called with additional tags
    /// Then only new tags should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_ExistingTags_AddsOnlyNewTags()
    {
        // Arrange
        var existingTag = CreateTag(1, "Groceries");
        var split = new TransactionSplit { Tags = [existingTag] };
        var newTags = new List<Tag>
        {
            CreateTag(1, "Groceries"), // Already exists
            CreateTag(2, "Food"),       // New
        };

        // Act
        split.UpdateTags(newTags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
    }

    #endregion

    #region UpdateTags - Removing Tags

    /// <summary>
    /// Given a split with tags
    /// When UpdateTags is called without some tags
    /// Then missing tags should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_MissingTags_RemovesTags()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Tags =
            [
                CreateTag(1, "Groceries"),
                CreateTag(2, "Food"),
                CreateTag(3, "Shopping"),
            ]
        };
        var remainingTags = new List<Tag>
        {
            CreateTag(1, "Groceries"), // Keep
            CreateTag(3, "Shopping"),  // Keep
        };

        // Act
        split.UpdateTags(remainingTags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
        Assert.DoesNotContain(split.Tags, t => t.Id == 2);
    }

    /// <summary>
    /// Given a split with tags
    /// When UpdateTags is called with empty list
    /// Then all tags should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_EmptyList_RemovesAllTags()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Tags =
            [
                CreateTag(1, "Groceries"),
                CreateTag(2, "Food"),
            ]
        };

        // Act
        split.UpdateTags([]);

        // Assert
        Assert.Empty(split.Tags);
    }

    #endregion

    #region UpdateTags - Mixed Operations

    /// <summary>
    /// Given a split with tags
    /// When UpdateTags is called with some new and some removed
    /// Then tags should be correctly added and removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_MixedChanges_AddAndRemoveCorrectly()
    {
        // Arrange
        var split = new TransactionSplit
        {
            Tags =
            [
                CreateTag(1, "Groceries"),   // Keep
                CreateTag(2, "OldTag"),      // Remove
            ]
        };
        var updatedTags = new List<Tag>
        {
            CreateTag(1, "Groceries"), // Keep
            CreateTag(3, "NewTag"),    // Add
        };

        // Act
        split.UpdateTags(updatedTags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
        Assert.Contains(split.Tags, t => t.Id == 1);
        Assert.Contains(split.Tags, t => t.Id == 3);
        Assert.DoesNotContain(split.Tags, t => t.Id == 2);
    }

    /// <summary>
    /// Given a split with tags
    /// When UpdateTags is called with same tags
    /// Then no changes should occur
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateTags_SameTags_NoChanges()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries");
        var tag2 = CreateTag(2, "Food");
        var split = new TransactionSplit { Tags = [tag1, tag2] };
        var sameTags = new List<Tag>
        {
            CreateTag(1, "Groceries"),
            CreateTag(2, "Food"),
        };

        // Act
        split.UpdateTags(sameTags);

        // Assert
        Assert.Equal(2, split.Tags.Count);
    }

    #endregion

    #region RemoveOffset

    /// <summary>
    /// Given a split with offsets
    /// When RemoveOffset is called
    /// Then the specified offset should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveOffset_RemovesSpecifiedOffset()
    {
        // Arrange
        var offset1 = new TransactionOffset { OffsetTransactionId = Guid.NewGuid(), Amount = 25m };
        var offset2 = new TransactionOffset { OffsetTransactionId = Guid.NewGuid(), Amount = 50m };
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy = [offset1, offset2]
        };

        // Act
        split.RemoveOffset(offset1);

        // Assert
        Assert.Single(split.OffsetBy);
        Assert.Contains(offset2, split.OffsetBy);
        Assert.DoesNotContain(offset1, split.OffsetBy);
    }

    /// <summary>
    /// Given a split with one offset
    /// When RemoveOffset is called
    /// Then the offsets collection should be empty
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveOffset_LastOffset_CollectionEmpty()
    {
        // Arrange
        var offset = new TransactionOffset { OffsetTransactionId = Guid.NewGuid(), Amount = 25m };
        var split = new TransactionSplit
        {
            Amount = 100m,
            OffsetBy = [offset]
        };

        // Act
        split.RemoveOffset(offset);

        // Assert
        Assert.Empty(split.OffsetBy);
    }

    #endregion

    private Tag CreateTag(int id, string name)
    {
        return new Tag(id)
        {
            Name = name,
            FamilyId = FamilyId,
        };
    }
}
