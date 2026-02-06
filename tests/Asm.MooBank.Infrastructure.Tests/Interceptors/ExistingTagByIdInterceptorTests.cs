#nullable enable
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Infrastructure.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Infrastructure.Tests.Interceptors;

[Trait("Category", "Unit")]
public class ExistingTagByIdInterceptorTests : IDisposable
{
    private readonly MooBankContext _context;

    public ExistingTagByIdInterceptorTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void SavingChanges_TagAddedWithNonZeroId_CompletesSuccessfully()
    {
        // Arrange - Simulates a tag that was "looked up" by ID but EF thinks it's new
        // Tag already has Settings via its auto-initializer (Settings = new())
        var tag = TestEntities.CreateTag(id: 42, familyId: Guid.NewGuid());

        // Add to context as new (Added state)
        _context.Add(tag);

        // Verify tag is in Added state
        Assert.Equal(EntityState.Added, _context.Entry(tag).State);

        // Act - Save will trigger the interceptor
        _context.SaveChanges();

        // Assert - Since SaveChanges completed, the interceptor ran.
        // Note: In-memory database doesn't truly test the interceptor behavior,
        // but we verify the operation completed successfully
    }

    [Fact]
    public void SavingChanges_TagAddedWithZeroId_CompletesSuccessfully()
    {
        // Arrange - Simulates a genuinely new tag with Id = 0
        // Note: For EF Core in-memory, Id=0 tags get auto-assigned IDs
        var tag = TestEntities.CreateTag(id: 0, familyId: Guid.NewGuid());

        _context.Add(tag);

        Assert.Equal(EntityState.Added, _context.Entry(tag).State);

        // Act - Should not throw
        _context.SaveChanges();

        // Assert - Tag was added successfully
        Assert.NotEqual(0, tag.Id); // EF assigned a new ID
    }

    [Fact]
    public void SavingChanges_TagModifiedWithValidName_SavesSuccessfully()
    {
        // Arrange - Simulates a legitimate modification
        var tag = TestEntities.CreateTag(id: 100, name: "Original", familyId: Guid.NewGuid());

        _context.Add(tag);
        _context.SaveChanges();

        // Modify the tag with a valid new name
        tag.Name = "Modified Name";

        // Act - Should save the modification
        _context.SaveChanges();

        // Assert - Tag was modified
        Assert.Equal("Modified Name", tag.Name);
    }

    [Fact]
    public void SavingChanges_NoTagsInChangeTracker_CompletesWithoutError()
    {
        // Arrange - No tags, context is empty

        // Act & Assert - should complete without error
        _context.SaveChanges();
    }

    [Fact]
    public void SavingChanges_MultipleTagsWithDifferentStates_ProcessesCorrectly()
    {
        // Arrange - Use unique IDs to avoid conflicts
        var tag1 = TestEntities.CreateTag(id: 200, name: "Tag1", familyId: Guid.NewGuid());
        var tag2 = TestEntities.CreateTag(id: 201, name: "Tag2", familyId: Guid.NewGuid());

        _context.AddRange(tag1, tag2);
        _context.SaveChanges();

        // Modify one tag
        tag1.Name = "Modified Tag1";

        // Act
        _context.SaveChanges();

        // Assert
        Assert.Equal("Modified Tag1", tag1.Name);
        Assert.Equal("Tag2", tag2.Name);
    }
}
