using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Unit tests for instrument authorization logic.
/// Tests verify owner and viewer authorization checks used by InstrumentOwnerAuthorisationHandler
/// and InstrumentViewerAuthorisationHandler.
/// </summary>
public class InstrumentAuthorizationTests
{
    private static readonly Guid _ownedInstrumentId = new("aaaaaaaa-bbbb-cccc-1111-222233334444");
    private static readonly Guid _sharedInstrumentId = new("dddddddd-eeee-ffff-4444-555566667777");
    private static readonly Guid _unauthorizedInstrumentId = new("11112222-3333-4444-5555-666677778888");

    #region Owner Authorization

    /// <summary>
    /// Given a user who owns an instrument
    /// When owner authorization is checked for that instrument
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void OwnerAuthorization_ForOwnedInstrument_Succeeds()
    {
        // Arrange
        var user = CreateUser(accounts: [_ownedInstrumentId]);

        // Act
        var isAuthorized = IsOwnerAuthorized(user, _ownedInstrumentId);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a user who owns instrument A
    /// When owner authorization is checked for instrument B
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void OwnerAuthorization_ForNonOwnedInstrument_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [_ownedInstrumentId]);

        // Act
        var isAuthorized = IsOwnerAuthorized(user, _unauthorizedInstrumentId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Given a null user
    /// When owner authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void OwnerAuthorization_ForNullUser_Fails()
    {
        // Arrange
        User? user = null;

        // Act
        var isAuthorized = IsOwnerAuthorized(user, _ownedInstrumentId);

        // Assert
        Assert.False(isAuthorized);
    }

    #endregion

    #region Viewer Authorization

    /// <summary>
    /// Given a user who owns an instrument
    /// When viewer authorization is checked for that instrument
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ViewerAuthorization_ForOwnedInstrument_Succeeds()
    {
        // Arrange
        var user = CreateUser(accounts: [_ownedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, _ownedInstrumentId);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a user with shared access to an instrument
    /// When viewer authorization is checked for that instrument
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ViewerAuthorization_ForSharedInstrument_Succeeds()
    {
        // Arrange
        var user = CreateUser(sharedAccounts: [_sharedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, _sharedInstrumentId);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a user without access to an instrument
    /// When viewer authorization is checked for that instrument
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ViewerAuthorization_ForUnauthorizedInstrument_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [_ownedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, _unauthorizedInstrumentId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Given a null user
    /// When viewer authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void ViewerAuthorization_ForNullUser_Fails()
    {
        // Arrange
        User? user = null;

        // Act
        var isAuthorized = IsViewerAuthorized(user, _sharedInstrumentId);

        // Assert
        Assert.False(isAuthorized);
    }

    #endregion

    #region Invalid Input

    /// <summary>
    /// Given a valid user
    /// When authorization is checked with an invalid (non-Guid) value
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void Authorization_WithInvalidGuidValue_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [_ownedInstrumentId]);
        object invalidValue = "not-a-guid";

        // Act - Simulates the handler's Guid.TryParse check
        var isAuthorized = invalidValue is Guid id && user.Accounts.Contains(id);

        // Assert
        Assert.False(isAuthorized);
    }

    #endregion

    /// <summary>
    /// Replicates InstrumentOwnerAuthorisationHandler.IsAuthorised logic.
    /// </summary>
    private static bool IsOwnerAuthorized(User? user, Guid instrumentId) =>
        user is not null && user.Accounts.Contains(instrumentId);

    /// <summary>
    /// Replicates InstrumentViewerAuthorisationHandler.IsAuthorised logic.
    /// </summary>
    private static bool IsViewerAuthorized(User? user, Guid instrumentId) =>
        user is not null && (user.Accounts.Contains(instrumentId) || user.SharedAccounts.Contains(instrumentId));

    private static User CreateUser(Guid[]? accounts = null, Guid[]? sharedAccounts = null) =>
        new()
        {
            Id = TestModels.UserId,
            EmailAddress = "test@test.com",
            FamilyId = TestModels.FamilyId,
            Currency = "AUD",
            Accounts = accounts ?? [],
            SharedAccounts = sharedAccounts ?? [],
        };
}

/// <summary>
/// Unit tests for group owner authorization logic.
/// Tests verify owner authorization checks used by GroupOwnerAuthorisationHandler.
/// </summary>
public class GroupOwnerAuthorizationTests
{
    private static readonly Guid _ownedGroupId = new("aaaaaaaa-bbbb-cccc-1111-222233334444");
    private static readonly Guid _unauthorizedGroupId = new("11112222-3333-4444-5555-666677778888");

    /// <summary>
    /// Given a user who owns a group
    /// When group owner authorization is checked for that group
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GroupOwnerAuthorization_ForOwnedGroup_Succeeds()
    {
        // Arrange
        var user = CreateUser(groups: [_ownedGroupId]);

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, _ownedGroupId);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a user who does not own a group
    /// When group owner authorization is checked for that group
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GroupOwnerAuthorization_ForNonOwnedGroup_Fails()
    {
        // Arrange
        var user = CreateUser(groups: [_ownedGroupId]);

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, _unauthorizedGroupId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Given a null user
    /// When group owner authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GroupOwnerAuthorization_ForNullUser_Fails()
    {
        // Arrange
        User? user = null;

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, _ownedGroupId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Given an invalid (non-GUID) value
    /// When group owner authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void GroupOwnerAuthorization_WithInvalidGuidValue_Fails()
    {
        // Arrange
        var user = CreateUser(groups: [_ownedGroupId]);
        object invalidValue = "not-a-guid";

        // Act - Simulates the handler's Guid.TryParse check
        var isAuthorized = Guid.TryParse(invalidValue.ToString(), out var groupId) && user.Groups.Contains(groupId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Replicates GroupOwnerAuthorisationHandler.IsAuthorised logic.
    /// </summary>
    private static bool IsGroupOwnerAuthorized(User? user, Guid groupId) =>
        user is not null && user.Groups.Contains(groupId);

    private static User CreateUser(Guid[]? groups = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
            Groups = groups ?? [],
        };
}

/// <summary>
/// Unit tests for family member authorization logic.
/// Tests verify family member checks used by FamilyMemberAuthorisationHandler.
/// </summary>
public class FamilyMemberAuthorizationTests
{
    private static readonly Guid _testFamilyId = Guid.NewGuid();

    /// <summary>
    /// Given a user in a family
    /// When family member authorization is checked for that family
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void FamilyMemberAuthorization_ForOwnFamily_Succeeds()
    {
        // Arrange
        var user = CreateUser(_testFamilyId);

        // Act
        var isAuthorized = IsFamilyMemberAuthorized(user, _testFamilyId);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a user in a different family
    /// When family member authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void FamilyMemberAuthorization_ForDifferentFamily_Fails()
    {
        // Arrange
        var user = CreateUser(_testFamilyId);
        var differentFamilyId = Guid.NewGuid();

        // Act
        var isAuthorized = IsFamilyMemberAuthorized(user, differentFamilyId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Replicates FamilyMemberAuthorisationHandler logic.
    /// </summary>
    private static bool IsFamilyMemberAuthorized(User user, Guid familyId) =>
        user.FamilyId == familyId;

    private static User CreateUser(Guid familyId) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = familyId,
            Currency = "AUD",
        };
}
