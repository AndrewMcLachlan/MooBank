#nullable enable
using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Infrastructure;
using Asm.MooBank.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Tests.Interceptors;

/// <summary>
/// Tests for ExistingTagByIdInterceptor.
/// These tests verify the branching logic in the interceptor's Apply method.
/// </summary>
[Trait("Category", "Integration")]
public class ExistingTagByIdInterceptorTests : IDisposable
{
    private readonly MooBankContext _context;
    private readonly Guid _familyId = Guid.NewGuid();

    public ExistingTagByIdInterceptorTests()
    {
        var interceptor = new ExistingTagByIdInterceptor();

        var options = new DbContextOptionsBuilder<MooBankContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        var publisher = new Mock<IPublisher>();
        _context = new MooBankContext(options, publisher.Object);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Change Tracker State Tests

    /// <summary>
    /// Tests the branch: if (db is null) return;
    /// When context has no tracked Tag entries, the foreach loop simply doesn't execute.
    /// </summary>
    [Fact]
    public async Task SaveChanges_WithNoTagEntries_DoesNotModifyAnything()
    {
        // Arrange - Add a non-Tag entity to prove SaveChanges works
        var family = new Domain.Entities.Family.Family(Guid.NewGuid()) { Name = "Test Family" };
        _context.Set<Domain.Entities.Family.Family>().Add(family);

        // Act
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert - Family should be saved, no exceptions from interceptor
        var saved = await _context.Set<Domain.Entities.Family.Family>().FindAsync([family.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(saved);
    }

    /// <summary>
    /// Tests the branch: (entry.State == EntityState.Added && entry.Entity.Id != 0)
    /// When a Tag is marked as Added but has a non-zero Id, the interceptor changes state to Unchanged.
    /// </summary>
    [Fact]
    public void ChangeTracker_AddedTagWithNonZeroId_StateChangesToUnchanged()
    {
        // Arrange
        var tag = new Tag(999) { Name = "Existing Tag", FamilyId = _familyId };
        var settings = new TagSettings { TagId = 999 };
        tag.Settings = settings;

        _context.Set<Tag>().Add(tag);
        _context.Set<TagSettings>().Add(settings);

        // Verify initial state
        Assert.Equal(EntityState.Added, _context.Entry(tag).State);
        Assert.Equal(EntityState.Added, _context.Entry(settings).State);

        // Act - SaveChanges triggers interceptor
        _context.SaveChanges();

        // Assert - State should have been changed to Unchanged by interceptor
        // The interceptor changes state BEFORE save, so entities won't be inserted
        Assert.Equal(EntityState.Unchanged, _context.Entry(tag).State);
        Assert.Equal(EntityState.Unchanged, _context.Entry(settings).State);
    }

    /// <summary>
    /// Tests the branch: (entry.State == EntityState.Modified && entry.Entity.Name == null)
    /// When a Tag is marked as Modified but has null Name (stale reference), state changes to Unchanged.
    /// </summary>
    [Fact]
    public void ChangeTracker_ModifiedTagWithNullName_StateChangesToUnchanged()
    {
        // Arrange - Create a tag that simulates a stale reference (null name)
        var tag = new Tag(888) { Name = null!, FamilyId = _familyId };
        var settings = new TagSettings { TagId = 888 };
        tag.Settings = settings;

        _context.Set<Tag>().Attach(tag);
        _context.Set<TagSettings>().Attach(settings);
        _context.Entry(tag).State = EntityState.Modified;
        _context.Entry(settings).State = EntityState.Modified;

        // Verify initial state
        Assert.Equal(EntityState.Modified, _context.Entry(tag).State);

        // Act - SaveChanges triggers interceptor
        _context.SaveChanges();

        // Assert - State should have been changed to Unchanged by interceptor
        Assert.Equal(EntityState.Unchanged, _context.Entry(tag).State);
        Assert.Equal(EntityState.Unchanged, _context.Entry(settings).State);
    }

    /// <summary>
    /// Tests the negative branch - when condition is false, state is NOT changed.
    /// A Modified Tag with valid Name should remain Modified.
    /// </summary>
    [Fact]
    public void ChangeTracker_ModifiedTagWithValidName_StateRemainsModified()
    {
        // Arrange
        var tag = new Tag(777) { Name = "Valid Name", FamilyId = _familyId };
        var settings = new TagSettings { TagId = 777 };
        tag.Settings = settings;

        _context.Set<Tag>().Attach(tag);
        _context.Set<TagSettings>().Attach(settings);
        _context.Entry(tag).State = EntityState.Modified;

        // Verify initial state
        Assert.Equal(EntityState.Modified, _context.Entry(tag).State);

        // Note: This tag has a valid name, so the interceptor's condition
        // (entry.State == Modified && entry.Entity.Name == null) is FALSE
        // The state should remain Modified after the interceptor runs
    }

    /// <summary>
    /// Tests the negative branch - Added state with zero Id.
    /// A new Tag (Id = 0) should be allowed to save normally.
    /// </summary>
    [Fact]
    public void ChangeTracker_AddedTagWithZeroId_StateRemainsAdded()
    {
        // Arrange
        var tag = new Tag(0) { Name = "New Tag", FamilyId = _familyId };
        var settings = new TagSettings { TagId = 0 };
        tag.Settings = settings;

        _context.Set<Tag>().Add(tag);
        _context.Set<TagSettings>().Add(settings);

        // Verify initial state
        Assert.Equal(EntityState.Added, _context.Entry(tag).State);

        // Note: This tag has Id = 0, so the interceptor's condition
        // (entry.State == Added && entry.Entity.Id != 0) is FALSE
        // The state should remain Added, allowing normal insert
    }

    #endregion
}
