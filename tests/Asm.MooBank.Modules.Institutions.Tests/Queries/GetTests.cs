using Asm.MooBank.Models;
using Asm.MooBank.Modules.Institutions.Queries;
using Asm.MooBank.Modules.Institutions.Tests.Support;

namespace Asm.MooBank.Modules.Institutions.Tests.Queries;

[Trait("Category", "Unit")]
public class GetTests
{
    [Fact]
    public async Task Handle_ExistingInstitution_ReturnsInstitution()
    {
        // Arrange
        var institution = TestEntities.CreateInstitution(1, "Test Bank", InstitutionType.Bank);
        var queryable = TestEntities.CreateInstitutionQueryable(institution);
        var handler = new GetHandler(queryable);
        var query = new Get(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Bank", result.Name);
        Assert.Equal(InstitutionType.Bank, result.InstitutionType);
    }

    [Fact]
    public async Task Handle_MultipleInstitutions_ReturnsCorrectOne()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetHandler(queryable);
        var query = new Get(3);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Gamma Building Society", result.Name);
        Assert.Equal(InstitutionType.BuildingSociety, result.InstitutionType);
    }

    [Fact]
    public async Task Handle_NonExistentInstitution_ThrowsNotFoundException()
    {
        // Arrange
        var institutions = TestEntities.CreateSampleInstitutions();
        var queryable = TestEntities.CreateInstitutionQueryable(institutions);
        var handler = new GetHandler(queryable);
        var query = new Get(999);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_EmptyCollection_ThrowsNotFoundException()
    {
        // Arrange
        var queryable = TestEntities.CreateInstitutionQueryable([]);
        var handler = new GetHandler(queryable);
        var query = new Get(1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None).AsTask());
    }
}
