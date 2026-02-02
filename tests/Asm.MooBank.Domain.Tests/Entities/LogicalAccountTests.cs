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
    /// Given a LogicalAccount with Import controller and institution accounts with ImporterTypeId
    /// When SetController is called with Manual
    /// Then ImporterTypeId should be cleared on all institution accounts
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetController_FromImportToManual_ClearsImporterTypeIds()
    {
        // Arrange
        var institutionAccount = new InstitutionAccount(Guid.NewGuid())
        {
            Name = "Bank",
            InstrumentId = TestModels.AccountId,
            ImporterTypeId = 1, // Has an importer set
        };
        var account = new LogicalAccount(TestModels.AccountId, [institutionAccount])
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
        Assert.Null(account.InstitutionAccounts.First().ImporterTypeId);
    }

    /// <summary>
    /// Given a LogicalAccount with Import controller
    /// When SetController is called with Import again
    /// Then ImporterTypeId should NOT be cleared
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void SetController_ToImport_KeepsImporterTypeIds()
    {
        // Arrange
        var institutionAccount = new InstitutionAccount(Guid.NewGuid())
        {
            Name = "Bank",
            InstrumentId = TestModels.AccountId,
            ImporterTypeId = 1,
        };
        var account = new LogicalAccount(TestModels.AccountId, [institutionAccount])
        {
            Name = "Test Account",
            Currency = "AUD",
            Controller = Controller.Manual,
            Owners = [new InstrumentOwner { UserId = TestModels.UserId, User = _entities.Owner }],
        };

        // Act
        account.SetController(Controller.Import);

        // Assert
        Assert.Equal(1, account.InstitutionAccounts.First().ImporterTypeId);
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
}
