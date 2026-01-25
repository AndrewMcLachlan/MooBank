#nullable enable

using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Tag"/> domain entity.
/// Tests cover equality, hashing, and comparison operations.
/// </summary>
public class TagTests
{
    private readonly TestEntities _entities = new();

    #region Equality

    /// <summary>
    /// Given two tags with the same ID
    /// When Equals is called
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(1, "Different Name");

        // Act & Assert
        Assert.True(tag1.Equals(tag2));
    }

    /// <summary>
    /// Given two tags with different IDs
    /// When Equals is called
    /// Then they should not be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Groceries");

        // Act & Assert
        Assert.False(tag1.Equals(tag2));
    }

    /// <summary>
    /// Given a tag
    /// When Equals is called with null
    /// Then it should return false
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Groceries");

        // Act & Assert
        Assert.False(tag.Equals(null));
    }

    /// <summary>
    /// Given a tag
    /// When Equals(object) is called with a Tag
    /// Then it should use Tag equality
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualsObject_WithTag_UsesTagEquality()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        object tag2 = _entities.CreateTag(1, "Same ID");

        // Act & Assert
        Assert.True(tag1.Equals(tag2));
    }

    /// <summary>
    /// Given a tag
    /// When Equals(object) is called with non-Tag
    /// Then it should return false
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualsObject_WithNonTag_ReturnsFalse()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Groceries");

        // Act & Assert
        Assert.False(tag.Equals("not a tag"));
    }

    #endregion

    #region Operators

    /// <summary>
    /// Given two tags with the same ID
    /// When compared with == operator
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualityOperator_SameId_ReturnsTrue()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(1, "Different Name");

        // Assert
        Assert.True(tag1 == tag2);
        Assert.False(tag1 != tag2);
    }

    /// <summary>
    /// Given two tags with different IDs
    /// When compared with != operator
    /// Then they should not be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void InequalityOperator_DifferentId_ReturnsTrue()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Food");

        // Assert
        Assert.True(tag1 != tag2);
        Assert.False(tag1 == tag2);
    }

    /// <summary>
    /// Given a null tag on left side
    /// When compared with == operator
    /// Then it should compare correctly
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualityOperator_LeftNull_ComparesCorrectly()
    {
        // Arrange
        Tag? tag1 = null;
        var tag2 = _entities.CreateTag(1, "Groceries");

        // Assert
        Assert.False(tag1 == tag2);
        Assert.True(tag1 != tag2);
    }

    /// <summary>
    /// Given both tags null
    /// When compared with == operator
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        // Arrange
        Tag? tag1 = null;
        Tag? tag2 = null;

        // Assert
        Assert.True(tag1 == tag2);
    }

    #endregion

    #region GetHashCode

    /// <summary>
    /// Given two tags with the same ID
    /// When GetHashCode is called
    /// Then they should have the same hash code
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_SameId_SameHashCode()
    {
        // Arrange
        var tag1 = _entities.CreateTag(42, "Groceries");
        var tag2 = _entities.CreateTag(42, "Different Name");

        // Assert
        Assert.Equal(tag1.GetHashCode(), tag2.GetHashCode());
    }

    /// <summary>
    /// Given two tags with different IDs
    /// When GetHashCode is called
    /// Then they should have different hash codes
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_DifferentId_DifferentHashCode()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Groceries");

        // Assert
        Assert.NotEqual(tag1.GetHashCode(), tag2.GetHashCode());
    }

    #endregion
}

/// <summary>
/// Unit tests for the <see cref="TagEqualityComparer"/> class.
/// </summary>
public class TagEqualityComparerTests
{
    private readonly TestEntities _entities = new();
    private readonly TagEqualityComparer _comparer = new();

    /// <summary>
    /// Given two tags with the same ID
    /// When comparer.Equals is called
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(1, "Different");

        // Act & Assert
        Assert.True(_comparer.Equals(tag1, tag2));
    }

    /// <summary>
    /// Given two tags with different IDs
    /// When comparer.Equals is called
    /// Then they should not be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_DifferentId_ReturnsFalse()
    {
        // Arrange
        var tag1 = _entities.CreateTag(1, "Groceries");
        var tag2 = _entities.CreateTag(2, "Food");

        // Act & Assert
        Assert.False(_comparer.Equals(tag1, tag2));
    }

    /// <summary>
    /// Given both tags are null
    /// When comparer.Equals is called
    /// Then they should be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(_comparer.Equals(null, null));
    }

    /// <summary>
    /// Given one tag is null
    /// When comparer.Equals is called
    /// Then they should not be equal
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Equals_OneNull_ReturnsFalse()
    {
        // Arrange
        var tag = _entities.CreateTag(1, "Groceries");

        // Act & Assert
        Assert.False(_comparer.Equals(tag, null));
        Assert.False(_comparer.Equals(null, tag));
    }

    /// <summary>
    /// Given a tag
    /// When comparer.GetHashCode is called
    /// Then it should return the ID's hash code
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetHashCode_ReturnsIdHashCode()
    {
        // Arrange
        var tag = _entities.CreateTag(42, "Test");

        // Act
        var hashCode = _comparer.GetHashCode(tag);

        // Assert
        Assert.Equal(42.GetHashCode(), hashCode);
    }
}
