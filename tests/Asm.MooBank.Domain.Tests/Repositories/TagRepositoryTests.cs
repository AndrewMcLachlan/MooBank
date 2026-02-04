using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Infrastructure.Repositories;

namespace Asm.MooBank.Domain.Tests.Repositories;

/// <summary>
/// Integration tests for the <see cref="TagRepository"/> class.
/// Tests verify tag CRUD operations against an in-memory database.
/// </summary>
public class TagRepositoryTests : IDisposable
{
    private readonly Infrastructure.MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();
    private readonly Models.User _user;

    public TagRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _user = TestDbContextFactory.CreateTestUser(_familyId);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Get All

    /// <summary>
    /// Given tags exist for a family
    /// When Get is called
    /// Then only tags for that family should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithExistingTags_ReturnsTagsForFamily()
    {
        // Arrange
        var tag1 = CreateTag(1, "Groceries", _familyId);
        var tag2 = CreateTag(2, "Entertainment", _familyId);
        var otherFamilyTag = CreateTag(3, "Other", Guid.NewGuid());

        _context.Set<Tag>().AddRange(tag1, tag2, otherFamilyTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(_familyId, t.FamilyId));
    }

    /// <summary>
    /// Given deleted tags exist
    /// When Get is called
    /// Then deleted tags should not be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Get_WithDeletedTags_ExcludesDeletedTags()
    {
        // Arrange
        var activeTag = CreateTag(1, "Active", _familyId);
        var deletedTag = CreateTag(2, "Deleted", _familyId, deleted: true);

        _context.Set<Tag>().AddRange(activeTag, deletedTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get(TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active", result.First().Name);
    }

    #endregion

    #region Get By Id

    /// <summary>
    /// Given a tag exists
    /// When Get by id is called
    /// Then the tag should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_ExistingTag_ReturnsTag()
    {
        // Arrange
        var tag = CreateTag(1, "Groceries", _familyId);
        _context.Set<Tag>().Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get(1, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Groceries", result.Name);
    }

    /// <summary>
    /// Given a tag does not exist
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_NonExistentTag_ThrowsNotFoundException()
    {
        // Arrange
        var repository = new TagRepository(_context, _user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(999, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Given a tag belongs to a different family
    /// When Get by id is called
    /// Then NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_TagFromDifferentFamily_ThrowsNotFoundException()
    {
        // Arrange
        var otherFamilyTag = CreateTag(1, "Other", Guid.NewGuid());
        _context.Set<Tag>().Add(otherFamilyTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => repository.Get(1, TestContext.Current.CancellationToken));
    }

    #endregion

    #region Get By Id with IncludeSubTags

    /// <summary>
    /// Given a tag with sub-tags exists
    /// When Get by id is called with includeSubTags = true
    /// Then the tag with sub-tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_WithIncludeSubTagsTrue_ReturnsTagWithSubTags()
    {
        // Arrange
        var parentTag = CreateTag(1, "Parent", _familyId);
        var childTag = CreateTag(2, "Child", _familyId);
        parentTag.Tags.Add(childTag);

        _context.Set<Tag>().AddRange(parentTag, childTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get(1, includeSubTags: true, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Parent", result.Name);
        Assert.Single(result.Tags);
        Assert.Equal("Child", result.Tags.First().Name);
    }

    /// <summary>
    /// Given a tag exists
    /// When Get by id is called with includeSubTags = false
    /// Then the tag without sub-tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetById_WithIncludeSubTagsFalse_ReturnsTagWithoutSubTags()
    {
        // Arrange
        var parentTag = CreateTag(1, "Parent", _familyId);
        var childTag = CreateTag(2, "Child", _familyId);
        parentTag.Tags.Add(childTag);

        _context.Set<Tag>().AddRange(parentTag, childTag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get(1, includeSubTags: false, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Parent", result.Name);
        // Sub-tags should not be loaded when includeSubTags = false
    }

    #endregion

    #region Get By Multiple Ids

    /// <summary>
    /// Given multiple tags exist
    /// When Get by ids is called
    /// Then only matching tags should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetByIds_WithMultipleIds_ReturnsMatchingTags()
    {
        // Arrange
        var tag1 = CreateTag(1, "Tag1", _familyId);
        var tag2 = CreateTag(2, "Tag2", _familyId);
        var tag3 = CreateTag(3, "Tag3", _familyId);

        _context.Set<Tag>().AddRange(tag1, tag2, tag3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        var result = await repository.Get([1, 3], TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == 1);
        Assert.Contains(result, t => t.Id == 3);
    }

    #endregion

    #region Add

    /// <summary>
    /// Given a new tag
    /// When Add is called
    /// Then the tag should be persisted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Add_NewTag_PersistsTag()
    {
        // Arrange
        var repository = new TagRepository(_context, _user);
        var tag = CreateTag(0, "New Tag", _familyId);

        // Act
        repository.Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedTag = await _context.Set<Tag>().FirstOrDefaultAsync(t => t.Name == "New Tag", TestContext.Current.CancellationToken);
        Assert.NotNull(savedTag);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Given an existing tag
    /// When Delete is called
    /// Then the tag should be marked as deleted (soft delete)
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Delete_ExistingTag_SoftDeletesTag()
    {
        // Arrange
        var tag = CreateTag(1, "ToDelete", _familyId);
        _context.Set<Tag>().Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        repository.Delete(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedTag = await _context.Set<Tag>().FirstOrDefaultAsync(t => t.Id == 1, TestContext.Current.CancellationToken);
        Assert.NotNull(deletedTag);
        Assert.True(deletedTag.Deleted);
    }

    #endregion

    #region AddSettings

    /// <summary>
    /// Given a tag without settings
    /// When AddSettings is called
    /// Then settings should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddSettings_TagWithoutSettings_CreatesSettings()
    {
        // Arrange
        var tag = new Tag(1)
        {
            Name = "Test",
            FamilyId = _familyId,
            Settings = null!,
        };
        _context.Set<Tag>().Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);

        // Act
        repository.AddSettings(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var reloadedTag = await _context.Set<Tag>().Include(t => t.Settings).FirstOrDefaultAsync(t => t.Id == 1, TestContext.Current.CancellationToken);
        Assert.NotNull(reloadedTag?.Settings);
    }

    /// <summary>
    /// Given a tag with existing settings
    /// When AddSettings is called
    /// Then no new settings should be created
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task AddSettings_TagWithSettings_DoesNothing()
    {
        // Arrange
        var tag = CreateTag(1, "Test", _familyId);
        _context.Set<Tag>().Add(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new TagRepository(_context, _user);
        var initialCount = await _context.Set<TagSettings>().CountAsync(TestContext.Current.CancellationToken);

        // Act
        repository.AddSettings(tag);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var finalCount = await _context.Set<TagSettings>().CountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(initialCount, finalCount);
    }

    #endregion

    private static Tag CreateTag(int id, string name, Guid familyId, bool deleted = false) =>
        new(id)
        {
            Name = name,
            FamilyId = familyId,
            Deleted = deleted,
            Settings = new TagSettings(id),
        };
}
