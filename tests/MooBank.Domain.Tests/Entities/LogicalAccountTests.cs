using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Tests.Support;
using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the <see cref="LogicalAccount"/> domain entity.
/// Tests cover institution account management, controller settings, and viewer validation.
/// </summary>
public class LogicalAccountTests
{
    private readonly TestEntities _entities = new();

    #region AddInstitutionAccount

    /// <summary>
    /// Given a LogicalAccount with no institution accounts
    /// When AddInstitutionAccount is called
    /// Then InstitutionAccounts.Count should be 1
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddInstitutionAccount_ToEmptyCollection_AddsAccount()
    {
        // Arrange
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };
        var institutionAccount = new InstitutionAccount(Guid.NewGuid())
        {
            Name = "Bank Account",
            InstrumentId = TestModels.AccountId,
        };

        // Act
        account.AddInstitutionAccount(institutionAccount);

        // Assert
        Assert.Single(account.InstitutionAccounts);
    }

    /// <summary>
    /// Given a LogicalAccount with existing institution accounts
    /// When AddInstitutionAccount is called
    /// Then the new account should be added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void AddInstitutionAccount_ToExistingCollection_AddsToEnd()
    {
        // Arrange
        var existingInstitutionAccount = new InstitutionAccount(Guid.NewGuid())
        {
            Name = "Existing Bank",
            InstrumentId = TestModels.AccountId,
        };
        var account = new LogicalAccount(TestModels.AccountId, [existingInstitutionAccount])
        {
            Name = "Test Account",
            Currency = "AUD",
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };
        var newInstitutionAccount = new InstitutionAccount(Guid.NewGuid())
        {
            Name = "New Bank",
            InstrumentId = TestModels.AccountId,
        };

        // Act
        account.AddInstitutionAccount(newInstitutionAccount);

        // Assert
        Assert.Equal(2, account.InstitutionAccounts.Count);
    }

    #endregion

    #region SetController

    /// <summary>
    /// Given a LogicalAccount with Manual controller
    /// When SetController is called with Import
    /// Then Controller should be Import
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetController_ToImport_SetsController()
    {
        // Arrange
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Controller = Controller.Manual,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };

        // Act
        account.SetController(Controller.Import);

        // Assert
        Assert.Equal(Controller.Import, account.Controller);
    }

    /// <summary>
    /// Given a LogicalAccount with Import controller
    /// When SetController is called with Manual
    /// Then Controller should be Manual
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetController_FromImportToManual_SetsController()
    {
        // Arrange
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            Controller = Controller.Import,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };

        // Act
        account.SetController(Controller.Manual);

        // Assert
        Assert.Equal(Controller.Manual, account.Controller);
    }

    #endregion

    #region ValidViewers

    /// <summary>
    /// Given a LogicalAccount with ShareWithFamily = false
    /// When ValidViewers is accessed
    /// Then it should return empty collection
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidViewers_ShareWithFamilyFalse_ReturnsEmpty()
    {
        // Arrange
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            ShareWithFamily = false,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = Guid.NewGuid(), User = _entities.FamilyUser }],
        };

        // Act
        var validViewers = account.ValidViewers;

        // Assert
        Assert.Empty(validViewers);
    }

    /// <summary>
    /// Given a LogicalAccount with ShareWithFamily = true and a viewer from the same family
    /// When ValidViewers is accessed
    /// Then it should include the family viewer
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidViewers_ShareWithFamilyTrue_IncludesFamilyViewer()
    {
        // Arrange
        var viewerId = Guid.NewGuid();
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            ShareWithFamily = true,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = viewerId, User = _entities.FamilyUser }],
        };

        // Act
        var validViewers = account.ValidViewers.ToList();

        // Assert
        Assert.Single(validViewers);
        Assert.Equal(viewerId, validViewers[0].UserId);
    }

    /// <summary>
    /// Given a LogicalAccount with ShareWithFamily = true and a viewer from a different family
    /// When ValidViewers is accessed
    /// Then it should NOT include the non-family viewer
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ValidViewers_ShareWithFamilyTrue_ExcludesNonFamilyViewer()
    {
        // Arrange
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            ShareWithFamily = true,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = Guid.NewGuid(), User = _entities.OtherUser }],
        };

        // Act
        var validViewers = account.ValidViewers;

        // Assert
        Assert.Empty(validViewers);
    }

    #endregion

    #region GetGroup (override)

    /// <summary>
    /// Given a LogicalAccount with a valid viewer that has a group
    /// When GetGroup is called for a viewer (not owner)
    /// Then it should return the viewer's group
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GetGroup_ForValidViewer_ReturnsViewerGroup()
    {
        // Arrange
        var viewerId = _entities.FamilyUser.Id;
        var group = new Domain.Entities.Group.Group(TestModels.GroupId)
        {
            Name = "Viewer Group",
            OwnerId = viewerId,
        };
        var account = new LogicalAccount(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
            ShareWithFamily = true,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
            Viewers = [new InstrumentViewer { UserId = viewerId, User = _entities.FamilyUser, GroupId = TestModels.GroupId, Group = group }],
        };

        // Act
        var result = account.GetGroup(viewerId);

        // Assert
        Assert.Equal(group, result);
    }

    #endregion

    #region SetTagPurpose

    /// <summary>
    /// Given a LogicalAccount with no tag purpose for a purpose
    /// When SetTagPurpose is called with a tag id
    /// Then a new tag purpose is added
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetTagPurpose_NoExisting_AddsTagPurpose()
    {
        // Arrange
        var account = CreateAccount();

        // Act
        account.SetTagPurpose(TagPurpose.Interest, 42);

        // Assert
        var tagPurpose = Assert.Single(account.TagPurposes);
        Assert.Equal(TagPurpose.Interest, tagPurpose.Purpose);
        Assert.Equal(42, tagPurpose.TagId);
    }

    /// <summary>
    /// Given a LogicalAccount with an existing tag purpose
    /// When SetTagPurpose is called for the same purpose with a different tag id
    /// Then the existing tag purpose's tag id is updated (not duplicated)
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetTagPurpose_Existing_UpdatesTagId()
    {
        // Arrange
        var account = CreateAccount();
        account.SetTagPurpose(TagPurpose.Interest, 42);

        // Act
        account.SetTagPurpose(TagPurpose.Interest, 99);

        // Assert
        var tagPurpose = Assert.Single(account.TagPurposes);
        Assert.Equal(99, tagPurpose.TagId);
    }

    /// <summary>
    /// Given a LogicalAccount with an existing tag purpose
    /// When SetTagPurpose is called for that purpose with a null tag id
    /// Then the existing tag purpose is removed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetTagPurpose_NullTagId_RemovesExisting()
    {
        // Arrange
        var account = CreateAccount();
        account.SetTagPurpose(TagPurpose.Interest, 42);

        // Act
        account.SetTagPurpose(TagPurpose.Interest, null);

        // Assert
        Assert.Empty(account.TagPurposes);
    }

    /// <summary>
    /// Given a LogicalAccount with a tag purpose for a different purpose
    /// When SetTagPurpose is called with a null tag id for a purpose that is not set
    /// Then nothing is removed and the unrelated purpose is left intact
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetTagPurpose_NullTagId_NoMatchingPurpose_LeavesOthersIntact()
    {
        // Arrange
        var account = CreateAccount();
        account.SetTagPurpose(TagPurpose.Interest, 42);

        // Act
        account.SetTagPurpose(TagPurpose.MortgageInterest, null);

        // Assert
        var tagPurpose = Assert.Single(account.TagPurposes);
        Assert.Equal(TagPurpose.Interest, tagPurpose.Purpose);
    }

    private static LogicalAccount CreateAccount() =>
        new(TestModels.AccountId, [])
        {
            Name = "Test Account",
            Currency = "AUD",
        };

    #endregion
}
