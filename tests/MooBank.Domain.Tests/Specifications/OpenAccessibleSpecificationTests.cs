using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Domain.Tests.Support;

namespace Asm.MooBank.Domain.Tests.Specifications;

/// <summary>
/// Unit tests for the <see cref="OpenAccessibleSpecification{T}"/> specification.
/// Tests verify filtering logic for owned, shared, and closed instruments.
/// </summary>
public class OpenAccessibleSpecificationTests
{
    private readonly TestEntities _entities = new();

    /// <summary>
    /// Given instruments where user owns 2 and others own 3
    /// When OpenAccessibleSpecification.Apply is called
    /// Then only 2 owned instruments should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithOwnedAndOtherInstruments_ReturnsOnlyOwned()
    {
        // Arrange
        var instruments = new List<LogicalAccount>
        {
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, null),
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, null),
            CreateInstrument(Guid.NewGuid(), TestModels.OtherFamilyId, false, null),
            CreateInstrument(Guid.NewGuid(), TestModels.OtherFamilyId, false, null),
            CreateInstrument(Guid.NewGuid(), TestModels.OtherFamilyId, false, null),
        };
        var spec = new OpenAccessibleSpecification<LogicalAccount>(TestModels.UserId, TestModels.FamilyId);

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given instruments where user owns 1 and family member shares 1 (ShareWithFamily = true)
    /// When OpenAccessibleSpecification.Apply is called
    /// Then 2 instruments should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithOwnedAndFamilyShared_ReturnsBoth()
    {
        // Arrange
        var familyMemberId = Guid.NewGuid();
        var instruments = new List<LogicalAccount>
        {
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, null),
            CreateInstrument(familyMemberId, TestModels.FamilyId, true, null), // Shared within family
        };
        var spec = new OpenAccessibleSpecification<LogicalAccount>(TestModels.UserId, TestModels.FamilyId);

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Equal(2, result.Count());
    }

    /// <summary>
    /// Given instruments where user owns 2 and 1 is closed
    /// When OpenAccessibleSpecification.Apply is called
    /// Then only 1 open instrument should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithOpenAndClosedInstruments_ReturnsOnlyOpen()
    {
        // Arrange
        var instruments = new List<LogicalAccount>
        {
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, null),
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, DateOnly.FromDateTime(DateTime.Today.AddDays(-1))),
        };
        var spec = new OpenAccessibleSpecification<LogicalAccount>(TestModels.UserId, TestModels.FamilyId);

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Single(result);
    }

    /// <summary>
    /// Given instruments where user owns 1 and non-family member shares 1
    /// When OpenAccessibleSpecification.Apply is called
    /// Then only 1 owned instrument should be returned (non-family shared excluded)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Apply_WithOwnedAndNonFamilyShared_ReturnsOnlyOwned()
    {
        // Arrange
        var instruments = new List<LogicalAccount>
        {
            CreateInstrument(TestModels.UserId, TestModels.FamilyId, false, null),
            CreateInstrument(Guid.NewGuid(), TestModels.OtherFamilyId, true, null), // Different family, shared
        };
        var spec = new OpenAccessibleSpecification<LogicalAccount>(TestModels.UserId, TestModels.FamilyId);

        // Act
        var result = spec.Apply(instruments.AsQueryable());

        // Assert
        Assert.Single(result);
    }

    private LogicalAccount CreateInstrument(Guid ownerId, Guid familyId, bool shareWithFamily, DateOnly? closedDate)
    {
        var owner = _entities.CreateUser(ownerId, familyId);

        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = _entities.Faker.Company.CompanyName(),
            Currency = "AUD",
            ShareWithFamily = shareWithFamily,
            ClosedDate = closedDate,
            Owners = [new InstrumentOwner { UserId = ownerId, User = owner }],
        };
    }
}
