#nullable enable
using Asm.MooBank.Modules.Families.Queries;
using Asm.MooBank.Modules.Families.Tests.Support;

namespace Asm.MooBank.Modules.Families.Tests.Queries;

[Trait("Category", "Unit")]
public class GetAllTests
{
    private readonly TestMocks _mocks;

    public GetAllTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_WithFamilies_ReturnsAllFamilies()
    {
        // Arrange
        var families = TestEntities.CreateSampleFamilies();
        var queryable = TestEntities.CreateFamilyQueryable(families);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetAllHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task Handle_NoFamilies_ReturnsEmptyList()
    {
        // Arrange
        var queryable = TestEntities.CreateFamilyQueryable([]);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetAllHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_OrdersByName()
    {
        // Arrange
        var families = new[]
        {
            TestEntities.CreateFamily(name: "Zebra Family"),
            TestEntities.CreateFamily(name: "Alpha Family"),
            TestEntities.CreateFamily(name: "Middle Family"),
        };
        var queryable = TestEntities.CreateFamilyQueryable(families);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetAllHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var resultList = result.ToList();
        Assert.Equal("Alpha Family", resultList[0].Name);
        Assert.Equal("Middle Family", resultList[1].Name);
        Assert.Equal("Zebra Family", resultList[2].Name);
    }

    [Fact]
    public async Task Handle_IncludesMembers()
    {
        // Arrange
        var member1 = TestEntities.CreateDomainUser(firstName: "John", lastName: "Doe");
        var member2 = TestEntities.CreateDomainUser(firstName: "Jane", lastName: "Doe");
        var family = TestEntities.CreateFamily(name: "Doe Family", members: [member1, member2]);
        var queryable = TestEntities.CreateFamilyQueryable(family);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetAllHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetAll();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var familyResult = result.First();
        Assert.Equal(2, familyResult.Members.Count());
    }

    [Fact]
    public async Task Handle_ValidQuery_AssertAdministrator()
    {
        // Arrange
        var families = TestEntities.CreateSampleFamilies();
        var queryable = TestEntities.CreateFamilyQueryable(families);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetAllHandler(queryable, _mocks.SecurityMock.Object);
        var query = new GetAll();

        // Act
        await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertAdministrator(It.IsAny<CancellationToken>()), Times.Once);
    }
}
