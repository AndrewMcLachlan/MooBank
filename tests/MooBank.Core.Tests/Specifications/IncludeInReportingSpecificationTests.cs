using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Tag.Specifications;

namespace Asm.MooBank.Core.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="IncludeInReportingSpecification"/> specification.
/// Tests verify tag filtering logic for reporting inclusion/exclusion.
/// </summary>
public class IncludeInReportingSpecificationTests
{
    private readonly TestEntities _entities = new();

    #region Basic Filtering

    /// <summary>
    /// Given a mix of active tags
    /// When IncludeInReportingSpecification is applied
    /// Then all non-deleted tags with reporting enabled should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithActiveTags_ReturnsAllActiveTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Groceries", deleted: false, excludeFromReporting: false),
            CreateTag(2, "Entertainment", deleted: false, excludeFromReporting: false),
            CreateTag(3, "Utilities", deleted: false, excludeFromReporting: false),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Equal(3, result.Count());
    }

    #endregion

    #region Deleted Tags

    /// <summary>
    /// Given a mix of deleted and active tags
    /// When IncludeInReportingSpecification is applied
    /// Then deleted tags should be excluded
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDeletedTags_ExcludesDeletedTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Groceries", deleted: false, excludeFromReporting: false),
            CreateTag(2, "Deleted Tag", deleted: true, excludeFromReporting: false),
            CreateTag(3, "Utilities", deleted: false, excludeFromReporting: false),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, t => t.Name == "Deleted Tag");
    }

    /// <summary>
    /// Given all deleted tags
    /// When IncludeInReportingSpecification is applied
    /// Then no tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAllDeletedTags_ReturnsEmpty()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Deleted 1", deleted: true, excludeFromReporting: false),
            CreateTag(2, "Deleted 2", deleted: true, excludeFromReporting: false),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Excluded From Reporting

    /// <summary>
    /// Given tags with some excluded from reporting
    /// When IncludeInReportingSpecification is applied
    /// Then excluded tags should not be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithExcludedTags_ExcludesExcludedFromReporting()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Groceries", deleted: false, excludeFromReporting: false),
            CreateTag(2, "Personal", deleted: false, excludeFromReporting: true),
            CreateTag(3, "Utilities", deleted: false, excludeFromReporting: false),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, t => t.Name == "Personal");
    }

    /// <summary>
    /// Given all tags excluded from reporting
    /// When IncludeInReportingSpecification is applied
    /// Then no tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithAllExcluded_ReturnsEmpty()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Excluded 1", deleted: false, excludeFromReporting: true),
            CreateTag(2, "Excluded 2", deleted: false, excludeFromReporting: true),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Null Settings

    /// <summary>
    /// Given a tag with null settings
    /// When IncludeInReportingSpecification is applied
    /// Then the tag should be included (null settings = not excluded)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithNullSettings_IncludesTag()
    {
        // Arrange
        var tagWithNullSettings = new Tag(1)
        {
            Name = "No Settings",
            FamilyId = TestModels.FamilyId,
            Deleted = false,
            Settings = null!,
        };

        var tagWithSettings = CreateTag(2, "Has Settings", deleted: false, excludeFromReporting: false);

        var tags = new List<Tag> { tagWithNullSettings, tagWithSettings };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Name == "No Settings");
    }

    #endregion

    #region Combined Conditions

    /// <summary>
    /// Given tags with various combinations of deleted and excluded
    /// When IncludeInReportingSpecification is applied
    /// Then only non-deleted and non-excluded tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithMixedConditions_ReturnsOnlyValidTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Active Included", deleted: false, excludeFromReporting: false),
            CreateTag(2, "Deleted", deleted: true, excludeFromReporting: false),
            CreateTag(3, "Excluded", deleted: false, excludeFromReporting: true),
            CreateTag(4, "Deleted And Excluded", deleted: true, excludeFromReporting: true),
            CreateTag(5, "Another Active", deleted: false, excludeFromReporting: false),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Name == "Active Included");
        Assert.Contains(result, t => t.Name == "Another Active");
    }

    /// <summary>
    /// Given a tag that is both deleted and excluded
    /// When IncludeInReportingSpecification is applied
    /// Then the tag should not be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithDeletedAndExcludedTag_ExcludesTag()
    {
        // Arrange
        var tags = new List<Tag>
        {
            CreateTag(1, "Active", deleted: false, excludeFromReporting: false),
            CreateTag(2, "Deleted And Excluded", deleted: true, excludeFromReporting: true),
        };

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result.First().Name);
    }

    #endregion

    #region Empty Input

    /// <summary>
    /// Given an empty collection of tags
    /// When IncludeInReportingSpecification is applied
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var tags = new List<Tag>();

        var spec = new IncludeInReportingSpecification();

        // Act
        var result = spec.Apply(tags.AsQueryable());

        // Assert
        Assert.Empty(result);
    }

    #endregion

    private Tag CreateTag(int id, string name, bool deleted, bool excludeFromReporting)
    {
        return new Tag(id)
        {
            Name = name,
            FamilyId = TestModels.FamilyId,
            Deleted = deleted,
            Settings = new TagSettings(id) { ExcludeFromReporting = excludeFromReporting },
        };
    }
}
