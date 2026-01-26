using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Domain;

/// <summary>
/// Unit tests for the <see cref="Instrument"/> domain entity.
/// Tests cover group assignment, account holder management, and virtual instrument operations.
/// </summary>
public class InstrumentTests
{
    private readonly TestEntities _entities = new();

    #region SetGroup

    /// <summary>
    /// Given an Instrument with an owner
    /// When SetGroup is called with the owner's userId
    /// Then the owner's GroupId should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ForOwner_UpdatesOwnerGroupId()
    {
        // Arrange
        var instrument = _entities.Account;

        // Act
        instrument.SetGroup(TestModels.GroupId, TestModels.UserId);

        // Assert
        var owner = instrument.Owners.Single(o => o.UserId == TestModels.UserId);
        Assert.Equal(TestModels.GroupId, owner.GroupId);
    }

    /// <summary>
    /// Given an Instrument without the user as a viewer
    /// When SetGroup is called with a non-owner userId
    /// Then a new InstrumentViewer should be added with the group
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ForNewViewer_CreatesViewerWithGroup()
    {
        // Arrange
        var viewerUserId = Guid.NewGuid();
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [],
        };

        // Act
        instrument.SetGroup(TestModels.GroupId, viewerUserId);

        // Assert
        var viewer = instrument.Viewers.SingleOrDefault(v => v.UserId == viewerUserId);
        Assert.NotNull(viewer);
        Assert.Equal(TestModels.GroupId, viewer.GroupId);
    }

    /// <summary>
    /// Given an Instrument with an existing viewer
    /// When SetGroup is called for that viewer
    /// Then the viewer's GroupId should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ForExistingViewer_UpdatesViewerGroupId()
    {
        // Arrange
        var viewerUserId = Guid.NewGuid();
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = viewerUserId, GroupId = null }],
        };

        // Act
        instrument.SetGroup(TestModels.GroupId, viewerUserId);

        // Assert
        var viewer = instrument.Viewers.Single(v => v.UserId == viewerUserId);
        Assert.Equal(TestModels.GroupId, viewer.GroupId);
    }

    #endregion

    #region SetAccountHolder

    /// <summary>
    /// Given an Instrument without the user as owner
    /// When SetAccountHolder is called
    /// Then the user should be added to Owners
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccountHolder_ForNewUser_AddsToOwners()
    {
        // Arrange
        var newOwnerId = Guid.NewGuid();
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };

        // Act
        instrument.SetAccountHolder(newOwnerId);

        // Assert
        Assert.Contains(instrument.Owners, o => o.UserId == newOwnerId);
    }

    /// <summary>
    /// Given an Instrument with the user already as owner
    /// When SetAccountHolder is called for the same user
    /// Then an ExistsException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccountHolder_ForExistingOwner_ThrowsExistsException()
    {
        // Arrange
        var instrument = _entities.Account;

        // Act & Assert
        var exception = Assert.Throws<ExistsException>(() => instrument.SetAccountHolder(TestModels.UserId));
        Assert.Equal("User is already an account holder", exception.Message);
    }

    #endregion

    #region AddVirtualInstrument / RemoveVirtualInstrument

    /// <summary>
    /// Given an Instrument with no virtual instruments
    /// When AddVirtualInstrument is called
    /// Then VirtualInstruments.Count should be 1
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_ToEmptyCollection_AddsVirtualInstrument()
    {
        // Arrange
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };
        var virtualInstrument = _entities.CreateVirtualInstrument();

        // Act
        instrument.AddVirtualInstrument(virtualInstrument, 100m);

        // Assert
        Assert.Single(instrument.VirtualInstruments);
    }

    /// <summary>
    /// Given an Instrument with a virtual instrument
    /// When RemoveVirtualInstrument is called with the correct ID
    /// Then VirtualInstruments.Count should be 0
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveVirtualInstrument_WithValidId_RemovesVirtualInstrument()
    {
        // Arrange
        var virtualInstrumentId = Guid.NewGuid();
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };
        instrument.AddVirtualInstrument(_entities.CreateVirtualInstrument(virtualInstrumentId), 0);

        // Act
        instrument.RemoveVirtualInstrument(virtualInstrumentId);

        // Assert
        Assert.Empty(instrument.VirtualInstruments);
    }

    /// <summary>
    /// Given an Instrument with no virtual instruments
    /// When RemoveVirtualInstrument is called with a non-existent ID
    /// Then a NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveVirtualInstrument_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };

        // Act & Assert
        Assert.Throws<NotFoundException>(() => instrument.RemoveVirtualInstrument(Guid.NewGuid()));
    }

    #endregion

    #region GetGroup

    /// <summary>
    /// Given an Instrument with an owner that has a group
    /// When GetGroup is called with the owner's userId
    /// Then the correct group should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForOwnerWithGroup_ReturnsGroup()
    {
        // Arrange
        var group = new Asm.MooBank.Domain.Entities.Group.Group(TestModels.GroupId)
        {
            Name = "Test Group",
            OwnerId = TestModels.UserId,
        };
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners =
            [
                new InstrumentOwner
                {
                    UserId = TestModels.UserId,
                    User = _entities.Owner,
                    GroupId = TestModels.GroupId,
                    Group = group,
                }
            ],
        };

        // Act
        var result = instrument.GetGroup(TestModels.UserId);

        // Assert
        Assert.Equal(group, result);
    }

    /// <summary>
    /// Given an Instrument with an owner that has no group
    /// When GetGroup is called with the owner's userId
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForOwnerWithoutGroup_ReturnsNull()
    {
        // Arrange
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners =
            [
                new InstrumentOwner
                {
                    UserId = TestModels.UserId,
                    User = _entities.Owner,
                    GroupId = null,
                    Group = null,
                }
            ],
        };

        // Act
        var result = instrument.GetGroup(TestModels.UserId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Given an Instrument with owners
    /// When GetGroup is called with a non-owner userId
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForNonOwner_ReturnsNull()
    {
        // Arrange
        var instrument = _entities.Account;

        // Act
        var result = instrument.GetGroup(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region PermittedUsers

    /// <summary>
    /// Given an Instrument with 1 owner and 1 viewer
    /// When PermittedUsers is accessed
    /// Then it should contain 2 user IDs
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void PermittedUsers_WithOwnerAndViewer_ContainsBothUsers()
    {
        // Arrange
        var viewerUserId = Guid.NewGuid();
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = viewerUserId }],
        };

        // Act
        var permittedUsers = instrument.PermittedUsers.ToList();

        // Assert
        Assert.Equal(2, permittedUsers.Count);
        Assert.Contains(TestModels.UserId, permittedUsers);
        Assert.Contains(viewerUserId, permittedUsers);
    }

    #endregion

    #region AddVirtualInstrument Events

    /// <summary>
    /// Given an Instrument
    /// When AddVirtualInstrument is called
    /// Then a VirtualInstrumentAddedEvent should be raised
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_RaisesVirtualInstrumentAddedEvent()
    {
        // Arrange
        var instrument = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };
        var virtualInstrument = _entities.CreateVirtualInstrument();

        // Act
        instrument.AddVirtualInstrument(virtualInstrument, 500m);

        // Assert
        Assert.Contains(instrument.Events, e => e.GetType().Name == "VirtualInstrumentAddedEvent");
    }

    #endregion
}
