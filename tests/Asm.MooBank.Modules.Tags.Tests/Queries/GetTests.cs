#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingTag_ReturnsTag()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries", familyId: familyId);
        var queryable = TestEntities.CreateTagQueryable(tag);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Groceries", result.Name);
    }

    [Fact]
    public async Task Handle_MultipleTags_ReturnsCorrectOne()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tags = new[]
        {
            TestEntities.CreateTag(id: 1, name: "Groceries", familyId: familyId),
            TestEntities.CreateTag(id: 2, name: "Fuel", familyId: familyId),
            TestEntities.CreateTag(id: 3, name: "Entertainment", familyId: familyId),
        };
        var queryable = TestEntities.CreateTagQueryable(tags);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(2);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Id);
        Assert.Equal("Fuel", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentTag_ThrowsNotFoundException()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tag = TestEntities.CreateTag(id: 1, name: "Groceries", familyId: familyId);
        var queryable = TestEntities.CreateTagQueryable(tag);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TagFromDifferentFamily_ThrowsNotFoundException()
    {
        // Arrange
        var otherFamilyId = Guid.NewGuid();
        var tag = TestEntities.CreateTag(id: 1, name: "Other Family Tag", familyId: otherFamilyId);
        var queryable = TestEntities.CreateTagQueryable(tag);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_TagWithSettings_ReturnsSettings()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tag = TestEntities.CreateTag(
            id: 1,
            name: "Smoothed Tag",
            familyId: familyId,
            applySmoothing: true,
            excludeFromReporting: true);
        var queryable = TestEntities.CreateTagQueryable(tag);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(1);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Settings.ApplySmoothing);
        Assert.True(result.Settings.ExcludeFromReporting);
    }

    [Fact]
    public async Task Handle_EmptyQueryable_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateTagQueryable([]);

        var handler = new GetHandler(queryable, _mocks.User);
        var query = new Get(1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }
}
