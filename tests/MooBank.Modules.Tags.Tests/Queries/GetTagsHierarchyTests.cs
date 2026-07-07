#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

/// <summary>
/// Tests for GetTagsHierarchy query handler.
/// Note: The handler returns root tags — tags where TaggedTo.Count == 0, meaning they have
/// no parent tag. The query then includes all their sub-tags via the Tags navigation property.
/// </summary>
[Trait("Category", "Unit")]
public class GetTagsHierarchyTests
{
    private readonly TestMocks _mocks;

    public GetTagsHierarchyTests()
    {
        _mocks = new TestMocks();
    }

    /// <summary>
    /// Given no tags
    /// When the hierarchy is requested
    /// Then an empty hierarchy is returned.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyTags_ReturnsEmptyHierarchy()
    {
        // Arrange
        var tags = TestEntities.CreateTagQueryable([]);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
        Assert.NotNull(result.Levels);
    }

    /// <summary>
    /// Given a tag with no parent
    /// When the hierarchy is requested
    /// Then the tag is returned as a root of the hierarchy.
    /// </summary>
    [Fact]
    public async Task Handle_RootTag_ReturnsTagInHierarchy()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Root tag with no parent (TaggedTo is empty)
        var rootTag = TestEntities.CreateTag(id: 1, name: "Category", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(rootTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal("Category", result.Tags.First().Name);
    }

    /// <summary>
    /// Given a tag with a parent
    /// When the hierarchy is requested
    /// Then the tag is not returned as a root of the hierarchy.
    /// </summary>
    [Fact]
    public async Task Handle_TagWithParent_IsNotIncludedAsRoot()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var parentCategory = TestEntities.CreateTag(id: 1, name: "Category", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "SubCategory", familyId: familyId);

        // childTag is "tagged to" parentCategory, so it is not a root
        childTag.TaggedTo.Add(parentCategory);
        parentCategory.Tags.Add(childTag);

        var tags = TestEntities.CreateTagQueryable(childTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Tags);
    }

    /// <summary>
    /// Given root tags in different families
    /// When the hierarchy is requested
    /// Then only tags in the user's family are returned.
    /// </summary>
    [Fact]
    public async Task Handle_FiltersByUserFamily()
    {
        // Arrange
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();

        var userTag = TestEntities.CreateTag(id: 1, name: "UserTag", familyId: userFamilyId);
        var otherTag = TestEntities.CreateTag(id: 2, name: "OtherTag", familyId: otherFamilyId);

        var tags = TestEntities.CreateTagQueryable(userTag, otherTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("UserTag", result.Tags.First().Name);
    }

    /// <summary>
    /// Given active and deleted root tags
    /// When the hierarchy is requested
    /// Then deleted tags are excluded.
    /// </summary>
    [Fact]
    public async Task Handle_ExcludesDeletedTags()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var activeTag = TestEntities.CreateTag(id: 1, name: "ActiveTag", familyId: familyId);
        var deletedTag = TestEntities.CreateTag(id: 2, name: "DeletedTag", familyId: familyId, deleted: true);

        var tags = TestEntities.CreateTagQueryable(activeTag, deletedTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("ActiveTag", result.Tags.First().Name);
    }

    /// <summary>
    /// Given a root tag
    /// When the hierarchy is requested
    /// Then five levels are always reported.
    /// </summary>
    [Fact]
    public async Task Handle_LevelsHas5Entries()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var tag = TestEntities.CreateTag(id: 1, name: "Tag", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(tag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        // The handler creates levels 1-5 regardless of actual depth
        Assert.Equal(5, result.Levels.Count);
        Assert.True(result.Levels.ContainsKey(1));
        Assert.True(result.Levels.ContainsKey(2));
        Assert.True(result.Levels.ContainsKey(3));
        Assert.True(result.Levels.ContainsKey(4));
        Assert.True(result.Levels.ContainsKey(5));
    }

    /// <summary>
    /// Given multiple root tags
    /// When the hierarchy is requested
    /// Then all root tags are returned.
    /// </summary>
    [Fact]
    public async Task Handle_MultipleRootTags_ReturnsAll()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var tag1 = TestEntities.CreateTag(id: 1, name: "Tag1", familyId: familyId);
        var tag2 = TestEntities.CreateTag(id: 2, name: "Tag2", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(tag1, tag2);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Tags.Count());
    }

    /// <summary>
    /// Given a root tag with sub-tags
    /// When the hierarchy is requested
    /// Then the sub-tags are included in the result.
    /// </summary>
    [Fact]
    public async Task Handle_RootTagWithSubTags_IncludesSubTagsInResult()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Create hierarchy: tag -> subTag1, subTag2
        var subTag1 = TestEntities.CreateTag(id: 2, name: "SubTag1", familyId: familyId);
        var subTag2 = TestEntities.CreateTag(id: 3, name: "SubTag2", familyId: familyId);

        var tag = TestEntities.CreateTag(id: 1, name: "Tag", familyId: familyId, subTags: [subTag1, subTag2]);

        var tags = TestEntities.CreateTagQueryable(tag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        var resultTag = result.Tags.First();
        Assert.Equal("Tag", resultTag.Name);
        Assert.Equal(2, resultTag.Tags.Count());
    }

    /// <summary>
    /// Given a root tag
    /// When the hierarchy is requested
    /// Then the tag model has the correct properties.
    /// </summary>
    [Fact]
    public async Task Handle_ReturnsTagModelWithCorrectProperties()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var tag = TestEntities.CreateTag(id: 2, name: "TestTag", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(tag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultTag = result.Tags.First();
        Assert.Equal(2, resultTag.Id);
        Assert.Equal("TestTag", resultTag.Name);
    }

    /// <summary>
    /// Given a root tag with two sub-tags
    /// When the hierarchy is requested
    /// Then the level counts reflect the nested hierarchy.
    /// </summary>
    [Fact]
    public async Task Handle_LevelsCounts_CorrectForNestedHierarchy()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var level1Sub1 = TestEntities.CreateTag(id: 2, name: "Level1Sub1", familyId: familyId);
        var level1Sub2 = TestEntities.CreateTag(id: 3, name: "Level1Sub2", familyId: familyId);

        var mainTag = TestEntities.CreateTag(id: 1, name: "MainTag", familyId: familyId, subTags: [level1Sub1, level1Sub2]);

        var tags = TestEntities.CreateTagQueryable(mainTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        // Level 1 should count the sub-tags of the main tag (2)
        Assert.Equal(2, result.Levels[1]);
        // Level 2 should be 0 (no sub-sub-tags)
        Assert.Equal(0, result.Levels[2]);
    }
}
