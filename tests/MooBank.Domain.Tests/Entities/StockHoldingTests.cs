using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="StockHolding"/> domain entity.
/// Tests cover stock-specific behavior and inheritance from Instrument.
/// </summary>
public class StockHoldingTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestFamilyId = Guid.NewGuid();

    #region ValidAccountViewers

    /// <summary>
    /// Given ShareWithFamily is false
    /// When ValidAccountViewers is accessed
    /// Then an empty collection should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidAccountViewers_ShareWithFamilyFalse_ReturnsEmpty()
    {
        // Arrange
        var holding = CreateStockHolding();
        holding.ShareWithFamily = false;

        // Act
        var viewers = holding.ValidAccountViewers;

        // Assert
        Assert.Empty(viewers);
    }

    /// <summary>
    /// Given ShareWithFamily is true and viewer is in same family as owner
    /// When ValidAccountViewers is accessed
    /// Then the viewer should be included
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidAccountViewers_ViewerInSameFamily_IncludesViewer()
    {
        // Arrange
        var holding = CreateStockHolding();
        holding.ShareWithFamily = true;

        var owner = CreateUser(TestUserId, TestFamilyId);
        var viewer = CreateUser(Guid.NewGuid(), TestFamilyId); // Same family

        holding.Owners.Add(new InstrumentOwner { UserId = owner.Id, User = owner });
        holding.Viewers.Add(new InstrumentViewer { UserId = viewer.Id, User = viewer });

        // Act
        var validViewers = holding.ValidAccountViewers.ToList();

        // Assert
        Assert.Single(validViewers);
        Assert.Equal(viewer.Id, validViewers[0].UserId);
    }

    /// <summary>
    /// Given ShareWithFamily is true but viewer is in different family
    /// When ValidAccountViewers is accessed
    /// Then the viewer should not be included
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidAccountViewers_ViewerInDifferentFamily_ExcludesViewer()
    {
        // Arrange
        var holding = CreateStockHolding();
        holding.ShareWithFamily = true;

        var owner = CreateUser(TestUserId, TestFamilyId);
        var viewer = CreateUser(Guid.NewGuid(), Guid.NewGuid()); // Different family

        holding.Owners.Add(new InstrumentOwner { UserId = owner.Id, User = owner });
        holding.Viewers.Add(new InstrumentViewer { UserId = viewer.Id, User = viewer });

        // Act
        var validViewers = holding.ValidAccountViewers.ToList();

        // Assert
        Assert.Empty(validViewers);
    }

    #endregion

    #region GetGroup

    /// <summary>
    /// Given a stock holding with an owner
    /// When GetGroup is called with the owner's ID
    /// Then the owner's group should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForOwner_ReturnsOwnerGroup()
    {
        // Arrange
        var holding = CreateStockHolding();
        var group = new Group(Guid.NewGuid()) { Name = "Test Group" };
        var owner = CreateUser(TestUserId, TestFamilyId);

        holding.Owners.Add(new InstrumentOwner { UserId = owner.Id, User = owner, Group = group });

        // Act
        var result = holding.GetGroup(owner.Id);

        // Assert
        Assert.Equal(group.Id, result?.Id);
    }

    /// <summary>
    /// Given a stock holding with a valid viewer
    /// When GetGroup is called with the viewer's ID
    /// Then the viewer's group should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForValidViewer_ReturnsViewerGroup()
    {
        // Arrange
        var holding = CreateStockHolding();
        holding.ShareWithFamily = true;

        var ownerGroup = new Group(Guid.NewGuid()) { Name = "Owner Group" };
        var viewerGroup = new Group(Guid.NewGuid()) { Name = "Viewer Group" };
        var owner = CreateUser(TestUserId, TestFamilyId);
        var viewer = CreateUser(Guid.NewGuid(), TestFamilyId); // Same family

        holding.Owners.Add(new InstrumentOwner { UserId = owner.Id, User = owner, Group = ownerGroup });
        holding.Viewers.Add(new InstrumentViewer { UserId = viewer.Id, User = viewer, Group = viewerGroup });

        // Act
        var result = holding.GetGroup(viewer.Id);

        // Assert
        Assert.Equal(viewerGroup.Id, result?.Id);
    }

    /// <summary>
    /// Given a stock holding
    /// When GetGroup is called with an unknown user ID
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForUnknownUser_ReturnsNull()
    {
        // Arrange
        var holding = CreateStockHolding();

        // Act
        var result = holding.GetGroup(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    private static StockHolding CreateStockHolding() =>
        new(Guid.NewGuid())
        {
            Name = "Test Stock",
            Currency = "USD",
            Symbol = new StockSymbolEntity("TEST", "US"),
        };

    private static User CreateUser(Guid userId, Guid familyId) =>
        new(userId)
        {
            EmailAddress = "test@test.com",
            Currency = "AUD",
            FamilyId = familyId,
        };
}
