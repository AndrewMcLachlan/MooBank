using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="User"/> domain entity.
/// Tests cover user properties and instrument relationships.
/// </summary>
public class UserTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestFamilyId = Guid.NewGuid();

    #region Instruments Property

    /// <summary>
    /// Given a user with instrument owners
    /// When Instruments is accessed
    /// Then all owned instruments should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Instruments_WithInstrumentOwners_ReturnsInstruments()
    {
        // Arrange
        var user = CreateUser();
        var instrument = CreateTestInstrument();
        user.InstrumentOwners.Add(new InstrumentOwner { UserId = user.Id, Instrument = instrument });

        // Act
        var instruments = user.Instruments.ToList();

        // Assert
        Assert.Single(instruments);
        Assert.Equal(instrument.Id, instruments[0].Id);
    }

    /// <summary>
    /// Given a user with no instrument owners
    /// When Instruments is accessed
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Instruments_NoInstrumentOwners_ReturnsEmpty()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var instruments = user.Instruments.ToList();

        // Assert
        Assert.Empty(instruments);
    }

    /// <summary>
    /// Given a user with null InstrumentOwners
    /// When Instruments is accessed
    /// Then empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Instruments_NullInstrumentOwners_ReturnsEmpty()
    {
        // Arrange
        var user = new User(TestUserId)
        {
            EmailAddress = "test@test.com",
            FamilyId = TestFamilyId,
            InstrumentOwners = null!,
        };

        // Act
        var instruments = user.Instruments.ToList();

        // Assert
        Assert.Empty(instruments);
    }

    #endregion

    #region Default Constructor

    /// <summary>
    /// Given a user created with default constructor
    /// When properties are accessed
    /// Then defaults should be correct
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Default_SetsDefaultCurrency()
    {
        // Arrange & Act
        var user = new User
        {
            EmailAddress = "test@test.com",
            FamilyId = TestFamilyId,
        };

        // Assert
        Assert.Equal("AUD", user.Currency);
    }

    #endregion

    private static User CreateUser() =>
        new(TestUserId)
        {
            EmailAddress = "test@test.com",
            Currency = "AUD",
            FamilyId = TestFamilyId,
        };

    private static Instrument CreateTestInstrument() =>
        new TestInstrument(Guid.NewGuid())
        {
            Name = "Test Account",
            Currency = "AUD",
        };
}

// File-scoped test instrument for mocking
file class TestInstrument(Guid id) : Instrument(id)
{
}
