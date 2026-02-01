#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithTags_ReturnsAllTags()
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

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_NoTags_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateTagQueryable([]);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FiltersToUserFamily()
    {
        // Arrange
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();
        var tags = new[]
        {
            TestEntities.CreateTag(id: 1, name: "User Tag 1", familyId: userFamilyId),
            TestEntities.CreateTag(id: 2, name: "User Tag 2", familyId: userFamilyId),
            TestEntities.CreateTag(id: 3, name: "Other Family Tag", familyId: otherFamilyId),
        };
        var queryable = TestEntities.CreateTagQueryable(tags);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Name.StartsWith("User Tag")));
    }

    [Fact]
    public async Task Handle_ExcludesDeletedTags()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tags = new[]
        {
            TestEntities.CreateTag(id: 1, name: "Active Tag", familyId: familyId, deleted: false),
            TestEntities.CreateTag(id: 2, name: "Deleted Tag", familyId: familyId, deleted: true),
        };
        var queryable = TestEntities.CreateTagQueryable(tags);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Tag", result.First().Name);
    }

    [Fact]
    public async Task Handle_OrdersByName()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var tags = new[]
        {
            TestEntities.CreateTag(id: 1, name: "Zebra", familyId: familyId),
            TestEntities.CreateTag(id: 2, name: "Apple", familyId: familyId),
            TestEntities.CreateTag(id: 3, name: "Mango", familyId: familyId),
        };
        var queryable = TestEntities.CreateTagQueryable(tags);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Equal("Apple", resultList[0].Name);
        Assert.Equal("Mango", resultList[1].Name);
        Assert.Equal("Zebra", resultList[2].Name);
    }

    [Fact]
    public async Task Handle_IncludesSubTags()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var subTag = TestEntities.CreateTag(id: 2, name: "Sub Tag", familyId: familyId);
        var parentTag = TestEntities.CreateTag(id: 1, name: "Parent Tag", familyId: familyId, subTags: [subTag]);
        var queryable = TestEntities.CreateTagQueryable(parentTag, subTag);

        var handler = new GetAllHandler(queryable, _mocks.User);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        var parentResult = result.FirstOrDefault(t => t.Name == "Parent Tag");
        Assert.NotNull(parentResult);
        Assert.Single(parentResult.Tags);
    }
}
