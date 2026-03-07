#nullable enable
using Asm.MooBank.Modules.Families.Queries;
using Asm.MooBank.Modules.Families.Tests.Support;

namespace Asm.MooBank.Modules.Families.Tests.Queries;

[Trait("Category", "Unit")]
public class GetMineTests
{
    private readonly TestMocks _mocks;

    public GetMineTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_UserHasFamily_ReturnsFamily()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var family = TestEntities.CreateFamily(id: familyId, name: "My Family");
        var queryable = TestEntities.CreateFamilyQueryable(family);

        var handler = new GetMineHandler(queryable, _mocks.User);
        var query = new GetMine();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(familyId, result.Id);
        Assert.Equal("My Family", result.Name);
    }

    [Fact]
    public async Task Handle_MultipleFamilies_ReturnsUserFamily()
    {
        // Arrange
        var userFamilyId = _mocks.User.FamilyId;
        var families = new[]
        {
            TestEntities.CreateFamily(name: "Other Family 1"),
            TestEntities.CreateFamily(id: userFamilyId, name: "User's Family"),
            TestEntities.CreateFamily(name: "Other Family 2"),
        };
        var queryable = TestEntities.CreateFamilyQueryable(families);

        var handler = new GetMineHandler(queryable, _mocks.User);
        var query = new GetMine();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(userFamilyId, result.Id);
        Assert.Equal("User's Family", result.Name);
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var otherFamily = TestEntities.CreateFamily(name: "Other Family");
        var queryable = TestEntities.CreateFamilyQueryable(otherFamily);

        var handler = new GetMineHandler(queryable, _mocks.User);
        var query = new GetMine();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyQueryable_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateFamilyQueryable([]);

        var handler = new GetMineHandler(queryable, _mocks.User);
        var query = new GetMine();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Handle_FamilyWithMembers_IncludesMembers()
    {
        // Arrange
        var familyId = _mocks.User.FamilyId;
        var member1 = TestEntities.CreateDomainUser(firstName: "John", lastName: "Smith");
        var member2 = TestEntities.CreateDomainUser(firstName: "Jane", lastName: "Smith");
        var family = TestEntities.CreateFamily(id: familyId, name: "Smith Family", members: [member1, member2]);
        var queryable = TestEntities.CreateFamilyQueryable(family);

        var handler = new GetMineHandler(queryable, _mocks.User);
        var query = new GetMine();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Members.Count());
    }
}
