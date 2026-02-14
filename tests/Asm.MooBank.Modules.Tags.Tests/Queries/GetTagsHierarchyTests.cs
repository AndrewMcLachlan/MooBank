#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

/// <summary>
/// Tests for GetTagsHierarchy query handler.
/// Note: The handler returns tags where TaggedTo.Count != 0, meaning it finds
/// tags that have at least one parent tag (they're attached to something).
/// The query then includes all their sub-tags via Tags navigation property.
/// </summary>
[Trait("Category", "Unit")]
public class GetTagsHierarchyTests
{
    private readonly TestMocks _mocks;

    public GetTagsHierarchyTests()
    {
        _mocks = new TestMocks();
    }

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

    [Fact]
    public async Task Handle_TagWithParent_ReturnsTagInHierarchy()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Create a tag that has a parent (TaggedTo is populated)
        var parentCategory = TestEntities.CreateTag(id: 1, name: "Category", familyId: familyId);
        var childTag = TestEntities.CreateTag(id: 2, name: "SubCategory", familyId: familyId);

        // childTag is "tagged to" parentCategory
        childTag.TaggedTo.Add(parentCategory);
        parentCategory.Tags.Add(childTag);

        var tags = TestEntities.CreateTagQueryable(childTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tags);
        Assert.Equal("SubCategory", result.Tags.First().Name);
    }

    [Fact]
    public async Task Handle_FiltersByUserFamily()
    {
        // Arrange
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();

        // Tag in user's family with a parent
        var userParent = TestEntities.CreateTag(id: 1, name: "UserParent", familyId: userFamilyId);
        var userTag = TestEntities.CreateTag(id: 2, name: "UserTag", familyId: userFamilyId);
        userTag.TaggedTo.Add(userParent);

        // Tag in other family with a parent
        var otherParent = TestEntities.CreateTag(id: 3, name: "OtherParent", familyId: otherFamilyId);
        var otherTag = TestEntities.CreateTag(id: 4, name: "OtherTag", familyId: otherFamilyId);
        otherTag.TaggedTo.Add(otherParent);

        var tags = TestEntities.CreateTagQueryable(userTag, otherTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("UserTag", result.Tags.First().Name);
    }

    [Fact]
    public async Task Handle_ExcludesDeletedTags()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Active tag with parent
        var parent1 = TestEntities.CreateTag(id: 1, name: "Parent1", familyId: familyId);
        var activeTag = TestEntities.CreateTag(id: 2, name: "ActiveTag", familyId: familyId);
        activeTag.TaggedTo.Add(parent1);

        // Deleted tag with parent
        var parent2 = TestEntities.CreateTag(id: 3, name: "Parent2", familyId: familyId);
        var deletedTag = TestEntities.CreateTag(id: 4, name: "DeletedTag", familyId: familyId, deleted: true);
        deletedTag.TaggedTo.Add(parent2);

        var tags = TestEntities.CreateTagQueryable(activeTag, deletedTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result.Tags);
        Assert.Equal("ActiveTag", result.Tags.First().Name);
    }

    [Fact]
    public async Task Handle_TagWithoutParent_IsNotIncluded()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Standalone tag with no parent (TaggedTo is empty)
        var standaloneTag = TestEntities.CreateTag(id: 1, name: "Standalone", familyId: familyId);
        // Note: TaggedTo is empty, so this tag should NOT be included

        var tags = TestEntities.CreateTagQueryable(standaloneTag);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task Handle_LevelsHas5Entries()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var tag = TestEntities.CreateTag(id: 2, name: "Tag", familyId: familyId);
        tag.TaggedTo.Add(parent);

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

    [Fact]
    public async Task Handle_MultipleTagsWithParents_ReturnsAll()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var parent1 = TestEntities.CreateTag(id: 1, name: "Parent1", familyId: familyId);
        var parent2 = TestEntities.CreateTag(id: 2, name: "Parent2", familyId: familyId);

        var tag1 = TestEntities.CreateTag(id: 3, name: "Tag1", familyId: familyId);
        var tag2 = TestEntities.CreateTag(id: 4, name: "Tag2", familyId: familyId);

        tag1.TaggedTo.Add(parent1);
        tag2.TaggedTo.Add(parent2);

        var tags = TestEntities.CreateTagQueryable(tag1, tag2);

        var handler = new GetTagsHierarchyHandler(tags, _mocks.User);

        var query = new GetTagsHierarchy();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Tags.Count());
    }

    [Fact]
    public async Task Handle_TagWithSubTags_IncludesSubTagsInResult()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Create hierarchy: parentCategory -> tag -> subTag1, subTag2
        var parentCategory = TestEntities.CreateTag(id: 1, name: "Category", familyId: familyId);

        var subTag1 = TestEntities.CreateTag(id: 3, name: "SubTag1", familyId: familyId);
        var subTag2 = TestEntities.CreateTag(id: 4, name: "SubTag2", familyId: familyId);

        var tag = TestEntities.CreateTag(id: 2, name: "Tag", familyId: familyId, subTags: [subTag1, subTag2]);

        // tag is "tagged to" parentCategory
        tag.TaggedTo.Add(parentCategory);

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

    [Fact]
    public async Task Handle_ReturnsTagModelWithCorrectProperties()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var tag = TestEntities.CreateTag(id: 2, name: "TestTag", familyId: familyId);
        tag.TaggedTo.Add(parent);

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

    [Fact]
    public async Task Handle_LevelsCounts_CorrectForNestedHierarchy()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;

        // Create tag with 2 sub-tags (level 1)
        var parentCategory = TestEntities.CreateTag(id: 1, name: "Category", familyId: familyId);

        var level1Sub1 = TestEntities.CreateTag(id: 3, name: "Level1Sub1", familyId: familyId);
        var level1Sub2 = TestEntities.CreateTag(id: 4, name: "Level1Sub2", familyId: familyId);

        var mainTag = TestEntities.CreateTag(id: 2, name: "MainTag", familyId: familyId, subTags: [level1Sub1, level1Sub2]);
        mainTag.TaggedTo.Add(parentCategory);

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
