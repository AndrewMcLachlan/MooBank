#nullable enable
using Asm.MooBank.Modules.Families.Queries;
using Asm.MooBank.Modules.Families.Tests.Support;

namespace Asm.MooBank.Modules.Families.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    private readonly TestMocks _mocks;

    public GetTests()
    {
        _mocks = new TestMocks();
    }

    [Fact]
    public async Task Handle_ExistingFamily_ReturnsFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family");
        var queryable = TestEntities.CreateFamilyQueryable(family);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetHandler(queryable, _mocks.SecurityMock.Object);
        var query = new Get(familyId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(familyId, result.Id);
        Assert.Equal("Test Family", result.Name);
    }

    [Fact]
    public async Task Handle_MultipleFamilies_ReturnsCorrectOne()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var families = new[]
        {
            TestEntities.CreateFamily(name: "Family 1"),
            TestEntities.CreateFamily(id: targetId, name: "Target Family"),
            TestEntities.CreateFamily(name: "Family 3"),
        };
        var queryable = TestEntities.CreateFamilyQueryable(families);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetHandler(queryable, _mocks.SecurityMock.Object);
        var query = new Get(targetId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(targetId, result.Id);
        Assert.Equal("Target Family", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentFamily_ThrowsNotFoundException()
    {
        // Arrange
        var family = TestEntities.CreateFamily(name: "Some Family");
        var queryable = TestEntities.CreateFamilyQueryable(family);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetHandler(queryable, _mocks.SecurityMock.Object);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyQueryable_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateFamilyQueryable([]);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetHandler(queryable, _mocks.SecurityMock.Object);
        var query = new Get(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ValidQuery_AssertAdministrator()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = TestEntities.CreateFamily(id: familyId, name: "Test Family");
        var queryable = TestEntities.CreateFamilyQueryable(family);

        _mocks.SecurityMock
            .Setup(s => s.AssertAdministrator(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new GetHandler(queryable, _mocks.SecurityMock.Object);
        var query = new Get(familyId);

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        _mocks.SecurityMock.Verify(s => s.AssertAdministrator(It.IsAny<CancellationToken>()), Times.Once);
    }
}
