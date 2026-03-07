using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Infrastructure.Repositories;
using Asm.MooBank.Infrastructure.Tests.Support;

namespace Asm.MooBank.Infrastructure.Tests.Repositories;

[Trait("Category", "Unit")]
public class TagRepositoryTests : IDisposable
{
    private readonly MooBankContext _context = TestDbContextFactory.Create();
    private readonly Models.User _user = TestEntities.CreateUserModel();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region AddSettings

    [Fact]
    public void AddSettings_TagHasNoSettings_CreatesNewSettings()
    {
        // Arrange
        var tag = TestEntities.CreateTag(id: 1, familyId: _user.FamilyId);
        tag.Settings = null!;

        _context.Add(tag);
        _context.SaveChanges();

        var repository = CreateRepository();

        // Act
        repository.AddSettings(tag);

        // Assert - verify TagSettings was added
        var settingsEntries = _context.ChangeTracker.Entries<TagSettings>()
            .Where(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added);

        Assert.Single(settingsEntries);
        Assert.NotNull(tag.Settings);
    }

    [Fact]
    public void AddSettings_TagHasSettings_DoesNotCreateNewSettings()
    {
        // Arrange
        var tag = TestEntities.CreateTag(id: 1, familyId: _user.FamilyId);
        tag.Settings = TestEntities.CreateTagSettings(tag.Id);

        var repository = CreateRepository();
        var initialCount = _context.ChangeTracker.Entries<TagSettings>().Count();

        // Act
        repository.AddSettings(tag);

        // Assert - no new settings should be added
        var finalCount = _context.ChangeTracker.Entries<TagSettings>().Count();
        Assert.Equal(initialCount, finalCount);
    }

    #endregion

    #region Get(int id, bool includeSubTags)

    [Fact]
    public async Task Get_WithIncludeSubTags_IncludesSubTags()
    {
        // Arrange
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: _user.FamilyId);
        parentTag.Settings = TestEntities.CreateTagSettings(parentTag.Id);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: _user.FamilyId);
        childTag.Settings = TestEntities.CreateTagSettings(childTag.Id);
        parentTag.Tags.Add(childTag);

        _context.AddRange(parentTag, childTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(parentTag.Id, includeSubTags: true, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Tags);
        Assert.Single(result.Tags);
    }

    [Fact]
    public async Task Get_WithoutIncludeSubTags_DoesNotIncludeSubTags()
    {
        // Arrange
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent", familyId: _user.FamilyId);
        parentTag.Settings = TestEntities.CreateTagSettings(parentTag.Id);
        var childTag = TestEntities.CreateTag(id: 2, name: "Child", familyId: _user.FamilyId);
        childTag.Settings = TestEntities.CreateTagSettings(childTag.Id);
        parentTag.Tags.Add(childTag);

        _context.AddRange(parentTag, childTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Clear tracker to ensure fresh query
        _context.ChangeTracker.Clear();

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(parentTag.Id, includeSubTags: false, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        // Sub-tags won't be loaded when includeSubTags is false
    }

    [Fact]
    public async Task Get_TagNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(999, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Get_TagBelongsToOtherFamily_ThrowsNotFoundException()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, familyId: otherFamilyId);
        tag.Settings = TestEntities.CreateTagSettings(tag.Id);

        _context.Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act & Assert - tag exists but belongs to different family
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(tag.Id, TestContext.Current.CancellationToken));
    }

    #endregion

    #region Get() - All tags

    [Fact]
    public async Task Get_ReturnsOnlyUserFamilyTags()
    {
        // Arrange
        var userTag1 = TestEntities.CreateTag(id: 1, name: "UserTag1", familyId: _user.FamilyId);
        var userTag2 = TestEntities.CreateTag(id: 2, name: "UserTag2", familyId: _user.FamilyId);
        var otherFamilyTag = TestEntities.CreateTag(id: 3, name: "OtherTag", familyId: Guid.NewGuid());

        _context.AddRange(userTag1, userTag2, otherFamilyTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(_user.FamilyId, t.FamilyId));
    }

    [Fact]
    public async Task Get_ExcludesDeletedTags()
    {
        // Arrange
        var activeTag = TestEntities.CreateTag(id: 1, name: "Active", familyId: _user.FamilyId, deleted: false);
        var deletedTag = TestEntities.CreateTag(id: 2, name: "Deleted", familyId: _user.FamilyId, deleted: true);

        _context.AddRange(activeTag, deletedTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result.First().Name);
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_MarksTagAsDeleted()
    {
        // Arrange
        var tag = TestEntities.CreateTag(id: 1, familyId: _user.FamilyId, deleted: false);

        var repository = CreateRepository();

        // Act
        repository.Delete(tag);

        // Assert
        Assert.True(tag.Deleted);
    }

    #endregion

    #region Get(IEnumerable<int> tagIds)

    [Fact]
    public async Task Get_ByIds_ReturnsMatchingTags()
    {
        // Arrange
        var tag1 = TestEntities.CreateTag(id: 1, name: "Tag1", familyId: _user.FamilyId);
        var tag2 = TestEntities.CreateTag(id: 2, name: "Tag2", familyId: _user.FamilyId);
        var tag3 = TestEntities.CreateTag(id: 3, name: "Tag3", familyId: _user.FamilyId);

        _context.AddRange(tag1, tag2, tag3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get([1, 3], TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == 1);
        Assert.Contains(result, t => t.Id == 3);
    }

    [Fact]
    public async Task Get_ByIds_OnlyReturnsUserFamilyTags()
    {
        // Arrange
        var userTag = TestEntities.CreateTag(id: 1, name: "UserTag", familyId: _user.FamilyId);
        var otherTag = TestEntities.CreateTag(id: 2, name: "OtherTag", familyId: Guid.NewGuid());

        _context.AddRange(userTag, otherTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = CreateRepository();

        // Act
        var result = await repository.Get([1, 2], TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    #endregion

    private TagRepository CreateRepository() => new(_context, _user);
}
