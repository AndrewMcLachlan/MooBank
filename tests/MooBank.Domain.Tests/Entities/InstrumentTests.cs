using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using DomainVirtualInstrument = Asm.MooBank.Domain.Entities.Account.VirtualInstrument;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="Instrument"/> entity.
/// Tests verify group management, account holder operations, and virtual instrument management.
/// Uses LogicalAccount as the concrete implementation for testing.
/// </summary>
public class InstrumentTests
{
    private static readonly Guid UserId1 = Guid.NewGuid();
    private static readonly Guid UserId2 = Guid.NewGuid();
    private static readonly Guid GroupId1 = Guid.NewGuid();
    private static readonly Guid GroupId2 = Guid.NewGuid();

    #region GetGroup

    /// <summary>
    /// Given an instrument with an owner that has a group
    /// When GetGroup is called with the owner's ID
    /// Then the group should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_OwnerHasGroup_ReturnsGroup()
    {
        // Arrange
        var group = new Asm.MooBank.Domain.Entities.Group.Group(GroupId1) { Name = "Test Group", OwnerId = Guid.NewGuid() };
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = GroupId1, Group = group });

        // Act
        var result = instrument.GetGroup(UserId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(GroupId1, result.Id);
    }

    /// <summary>
    /// Given an instrument with an owner that has no group
    /// When GetGroup is called with the owner's ID
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_OwnerHasNoGroup_ReturnsNull()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = null });

        // Act
        var result = instrument.GetGroup(UserId1);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Given an instrument with no matching owner
    /// When GetGroup is called with a non-owner ID
    /// Then null should be returned
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_UserNotOwner_ReturnsNull()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = GroupId1 });

        // Act
        var result = instrument.GetGroup(UserId2); // Different user

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SetGroup - Owner Scenarios

    /// <summary>
    /// Given an instrument with an existing owner
    /// When SetGroup is called for that owner
    /// Then the owner's group should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ExistingOwner_UpdatesOwnerGroup()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = null });

        // Act
        instrument.SetGroup(GroupId1, UserId1);

        // Assert
        var owner = instrument.Owners.Single(o => o.UserId == UserId1);
        Assert.Equal(GroupId1, owner.GroupId);
    }

    /// <summary>
    /// Given an instrument with an owner that has a group
    /// When SetGroup is called with a different group
    /// Then the owner's group should be changed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ExistingOwnerWithGroup_ChangesGroup()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = GroupId1 });

        // Act
        instrument.SetGroup(GroupId2, UserId1);

        // Assert
        var owner = instrument.Owners.Single(o => o.UserId == UserId1);
        Assert.Equal(GroupId2, owner.GroupId);
    }

    /// <summary>
    /// Given an instrument with an owner that has a group
    /// When SetGroup is called with null
    /// Then the owner's group should be cleared
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ExistingOwnerClearGroup_SetsNull()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1, GroupId = GroupId1 });

        // Act
        instrument.SetGroup(null, UserId1);

        // Assert
        var owner = instrument.Owners.Single(o => o.UserId == UserId1);
        Assert.Null(owner.GroupId);
    }

    #endregion

    #region SetGroup - Viewer Scenarios

    /// <summary>
    /// Given an instrument with an existing viewer (not owner)
    /// When SetGroup is called for that viewer
    /// Then the viewer's group should be updated
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_ExistingViewer_UpdatesViewerGroup()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Viewers.Add(new InstrumentViewer { UserId = UserId1, GroupId = null });

        // Act
        instrument.SetGroup(GroupId1, UserId1);

        // Assert
        var viewer = instrument.Viewers.Single(v => v.UserId == UserId1);
        Assert.Equal(GroupId1, viewer.GroupId);
    }

    /// <summary>
    /// Given an instrument with no owner or viewer for a user
    /// When SetGroup is called for that user
    /// Then a new viewer should be created with the group
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_NewUser_CreatesViewer()
    {
        // Arrange
        var instrument = CreateInstrument();

        // Act
        instrument.SetGroup(GroupId1, UserId1);

        // Assert
        Assert.Single(instrument.Viewers);
        var viewer = instrument.Viewers.First();
        Assert.Equal(UserId1, viewer.UserId);
        Assert.Equal(GroupId1, viewer.GroupId);
    }

    /// <summary>
    /// Given an instrument with no owner or viewer for a user
    /// When SetGroup is called with null group
    /// Then a new viewer should be created with null group
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetGroup_NewUserNullGroup_CreatesViewerWithNullGroup()
    {
        // Arrange
        var instrument = CreateInstrument();

        // Act
        instrument.SetGroup(null, UserId1);

        // Assert
        Assert.Single(instrument.Viewers);
        var viewer = instrument.Viewers.First();
        Assert.Equal(UserId1, viewer.UserId);
        Assert.Null(viewer.GroupId);
    }

    #endregion

    #region SetAccountHolder

    /// <summary>
    /// Given an instrument with no owners
    /// When SetAccountHolder is called
    /// Then a new owner should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccountHolder_NewUser_AddsOwner()
    {
        // Arrange
        var instrument = CreateInstrument();

        // Act
        instrument.SetAccountHolder(UserId1);

        // Assert
        Assert.Single(instrument.Owners);
        Assert.Equal(UserId1, instrument.Owners.First().UserId);
    }

    /// <summary>
    /// Given an instrument with an existing owner
    /// When SetAccountHolder is called for the same user
    /// Then an ExistsException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccountHolder_ExistingOwner_ThrowsExistsException()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1 });

        // Act & Assert
        var exception = Assert.Throws<ExistsException>(() => instrument.SetAccountHolder(UserId1));
        Assert.Equal("User is already an account holder", exception.Message);
    }

    /// <summary>
    /// Given an instrument with one owner
    /// When SetAccountHolder is called for a different user
    /// Then a second owner should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetAccountHolder_DifferentUser_AddsSecondOwner()
    {
        // Arrange
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = UserId1 });

        // Act
        instrument.SetAccountHolder(UserId2);

        // Assert
        Assert.Equal(2, instrument.Owners.Count);
        Assert.Contains(instrument.Owners, o => o.UserId == UserId1);
        Assert.Contains(instrument.Owners, o => o.UserId == UserId2);
    }

    #endregion

    #region AddVirtualInstrument

    /// <summary>
    /// Given an instrument with no virtual instruments
    /// When AddVirtualInstrument is called
    /// Then the virtual instrument should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_AddsToCollection()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument = CreateVirtualInstrument("Savings Goal");

        // Act
        instrument.AddVirtualInstrument(virtualInstrument, 100m);

        // Assert
        Assert.Single(instrument.VirtualInstruments);
        Assert.Contains(virtualInstrument, instrument.VirtualInstruments);
    }

    /// <summary>
    /// Given an instrument
    /// When AddVirtualInstrument is called
    /// Then a VirtualInstrumentAddedEvent should be raised
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_RaisesEvent()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument = CreateVirtualInstrument("Savings Goal");

        // Act
        instrument.AddVirtualInstrument(virtualInstrument, 500m);

        // Assert
        Assert.Single(instrument.Events);
        Assert.IsType<VirtualInstrumentAddedEvent>(instrument.Events.First());
    }

    /// <summary>
    /// Given an instrument
    /// When AddVirtualInstrument is called
    /// Then the event should contain the virtual instrument and opening balance
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_EventContainsCorrectData()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument = CreateVirtualInstrument("Savings Goal");
        var openingBalance = 750m;

        // Act
        instrument.AddVirtualInstrument(virtualInstrument, openingBalance);

        // Assert
        var addedEvent = instrument.Events.First() as VirtualInstrumentAddedEvent;
        Assert.NotNull(addedEvent);
        Assert.Same(virtualInstrument, addedEvent.Instrument);
        Assert.Equal(openingBalance, addedEvent.OpeningBalance);
    }

    /// <summary>
    /// Given an instrument with existing virtual instruments
    /// When AddVirtualInstrument is called
    /// Then the new virtual instrument should be added to existing ones
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddVirtualInstrument_AddsToExisting()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument1 = CreateVirtualInstrument("Goal 1");
        var virtualInstrument2 = CreateVirtualInstrument("Goal 2");
        instrument.AddVirtualInstrument(virtualInstrument1, 100m);

        // Act
        instrument.AddVirtualInstrument(virtualInstrument2, 200m);

        // Assert
        Assert.Equal(2, instrument.VirtualInstruments.Count);
    }

    #endregion

    #region RemoveVirtualInstrument

    /// <summary>
    /// Given an instrument with a virtual instrument
    /// When RemoveVirtualInstrument is called with the correct ID
    /// Then the virtual instrument should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveVirtualInstrument_ExistingInstrument_Removes()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument = CreateVirtualInstrument("Savings Goal");
        instrument.AddVirtualInstrument(virtualInstrument, 100m);

        // Act
        instrument.RemoveVirtualInstrument(virtualInstrument.Id);

        // Assert
        Assert.Empty(instrument.VirtualInstruments);
    }

    /// <summary>
    /// Given an instrument with no matching virtual instrument
    /// When RemoveVirtualInstrument is called
    /// Then a NotFoundException should be thrown
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveVirtualInstrument_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var instrument = CreateInstrument();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(() => instrument.RemoveVirtualInstrument(nonExistentId));
        Assert.Equal("Virtual instrument not found", exception.Message);
    }

    /// <summary>
    /// Given an instrument with multiple virtual instruments
    /// When RemoveVirtualInstrument is called for one
    /// Then only that virtual instrument should be removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveVirtualInstrument_MultipleInstruments_RemovesOnlySpecified()
    {
        // Arrange
        var instrument = CreateInstrument();
        var virtualInstrument1 = CreateVirtualInstrument("Goal 1");
        var virtualInstrument2 = CreateVirtualInstrument("Goal 2");
        instrument.AddVirtualInstrument(virtualInstrument1, 100m);
        instrument.AddVirtualInstrument(virtualInstrument2, 200m);

        // Act
        instrument.RemoveVirtualInstrument(virtualInstrument1.Id);

        // Assert
        Assert.Single(instrument.VirtualInstruments);
        Assert.Contains(virtualInstrument2, instrument.VirtualInstruments);
        Assert.DoesNotContain(virtualInstrument1, instrument.VirtualInstruments);
    }

    #endregion

    #region PermittedUsers

    /// <summary>
    /// Given an instrument with owners and viewers
    /// When PermittedUsers is accessed
    /// Then it should return all owner and viewer user IDs
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void PermittedUsers_ReturnsOwnersAndViewers()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var instrument = CreateInstrument();
        instrument.Owners.Add(new InstrumentOwner { UserId = ownerId });
        instrument.Viewers.Add(new InstrumentViewer { UserId = viewerId });

        // Act
        var permittedUsers = instrument.PermittedUsers.ToList();

        // Assert
        Assert.Equal(2, permittedUsers.Count);
        Assert.Contains(ownerId, permittedUsers);
        Assert.Contains(viewerId, permittedUsers);
    }

    /// <summary>
    /// Given an instrument with no owners or viewers
    /// When PermittedUsers is accessed
    /// Then it should return empty
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void PermittedUsers_NoUsers_ReturnsEmpty()
    {
        // Arrange
        var instrument = CreateInstrument();

        // Act
        var permittedUsers = instrument.PermittedUsers.ToList();

        // Assert
        Assert.Empty(permittedUsers);
    }

    #endregion

    private LogicalAccount CreateInstrument()
    {
        return new LogicalAccount(Guid.NewGuid(), [])
        {
            Name = "Test Account",
            Currency = "AUD",
            AccountType = AccountType.Transaction,
        };
    }

    private DomainVirtualInstrument CreateVirtualInstrument(string name)
    {
        return new DomainVirtualInstrument(Guid.NewGuid())
        {
            Name = name,
            Currency = "AUD",
            ParentInstrumentId = Guid.NewGuid(),
        };
    }
}
