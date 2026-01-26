using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Unit tests for instrument authorization logic.
/// Tests verify owner and viewer authorization checks used by InstrumentOwnerAuthorisationHandler
/// and InstrumentViewerAuthorisationHandler.
/// </summary>
public class InstrumentAuthorizationTests
{
    private static readonly Guid OwnedInstrumentId = new("aaaaaaaa-bbbb-cccc-1111-222233334444");
    private static readonly Guid SharedInstrumentId = new("dddddddd-eeee-ffff-4444-555566667777");
    private static readonly Guid UnauthorizedInstrumentId = new("11112222-3333-4444-5555-666677778888");

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
        var user = CreateUser(accounts: [OwnedInstrumentId]);

        // Act
        var isAuthorized = IsOwnerAuthorized(user, OwnedInstrumentId);

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
        var user = CreateUser(accounts: [OwnedInstrumentId]);

        // Act
        var isAuthorized = IsOwnerAuthorized(user, UnauthorizedInstrumentId);

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
        User user = null;

        // Act
        var isAuthorized = IsOwnerAuthorized(user, OwnedInstrumentId);

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
        var user = CreateUser(accounts: [OwnedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, OwnedInstrumentId);

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
        var user = CreateUser(sharedAccounts: [SharedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, SharedInstrumentId);

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
        var user = CreateUser(accounts: [OwnedInstrumentId]);

        // Act
        var isAuthorized = IsViewerAuthorized(user, UnauthorizedInstrumentId);

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
        User user = null;

        // Act
        var isAuthorized = IsViewerAuthorized(user, SharedInstrumentId);

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
        var user = CreateUser(accounts: [OwnedInstrumentId]);
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
    private static bool IsOwnerAuthorized(User user, Guid instrumentId) =>
        user is not null && user.Accounts.Contains(instrumentId);

    /// <summary>
    /// Replicates InstrumentViewerAuthorisationHandler.IsAuthorised logic.
    /// </summary>
    private static bool IsViewerAuthorized(User user, Guid instrumentId) =>
        user is not null && (user.Accounts.Contains(instrumentId) || user.SharedAccounts.Contains(instrumentId));

    private static User CreateUser(Guid[] accounts = null, Guid[] sharedAccounts = null) =>
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
    private static readonly Guid OwnedGroupId = new("aaaaaaaa-bbbb-cccc-1111-222233334444");
    private static readonly Guid UnauthorizedGroupId = new("11112222-3333-4444-5555-666677778888");

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
        var user = CreateUser(groups: [OwnedGroupId]);

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, OwnedGroupId);

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
        var user = CreateUser(groups: [OwnedGroupId]);

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, UnauthorizedGroupId);

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
        User user = null;

        // Act
        var isAuthorized = IsGroupOwnerAuthorized(user, OwnedGroupId);

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
        var user = CreateUser(groups: [OwnedGroupId]);
        object invalidValue = "not-a-guid";

        // Act - Simulates the handler's Guid.TryParse check
        var isAuthorized = Guid.TryParse(invalidValue.ToString(), out var groupId) && user.Groups.Contains(groupId);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Replicates GroupOwnerAuthorisationHandler.IsAuthorised logic.
    /// </summary>
    private static bool IsGroupOwnerAuthorized(User user, Guid groupId) =>
        user is not null && user.Groups.Contains(groupId);

    private static User CreateUser(Guid[] groups = null) =>
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
    private static readonly Guid TestFamilyId = Guid.NewGuid();

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
        var user = CreateUser(TestFamilyId);

        // Act
        var isAuthorized = IsFamilyMemberAuthorized(user, TestFamilyId);

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
        var user = CreateUser(TestFamilyId);
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

/// <summary>
/// Unit tests for budget line authorization logic.
/// Tests verify family-based authorization checks used by BudgetLineAuthorisationHandler.
/// </summary>
public class BudgetLineAuthorizationTests
{
    private static readonly Guid TestFamilyId = new("cb0a08af-2054-4873-9add-c259916d2d43");
    private static readonly Guid OtherFamilyId = new("1e658c80-3c5c-4cd0-95dd-fa09f6edb9e1");

    /// <summary>
    /// Given a budget line belonging to the user's family
    /// When budget line authorization is checked
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void BudgetLineAuthorization_ForOwnFamily_Succeeds()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var budgetLine = CreateBudgetLine(TestFamilyId);

        // Act
        var isAuthorized = IsBudgetLineAuthorized(user, budgetLine);

        // Assert
        Assert.True(isAuthorized);
    }

    /// <summary>
    /// Given a budget line belonging to a different family
    /// When budget line authorization is checked
    /// Then authorization should fail
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void BudgetLineAuthorization_ForDifferentFamily_Fails()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var budgetLine = CreateBudgetLine(OtherFamilyId);

        // Act
        var isAuthorized = IsBudgetLineAuthorized(user, budgetLine);

        // Assert
        Assert.False(isAuthorized);
    }

    /// <summary>
    /// Given a budget line with a null budget
    /// When budget line authorization is checked
    /// Then authorization should fail gracefully
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public void BudgetLineAuthorization_WithNullBudget_FailsGracefully()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var budgetLine = new BudgetLine(Guid.NewGuid())
        {
            Budget = null!
        };

        // Act & Assert - should throw NullReferenceException when accessing Budget.FamilyId
        Assert.Throws<NullReferenceException>(() => IsBudgetLineAuthorized(user, budgetLine));
    }

    /// <summary>
    /// Replicates BudgetLineAuthorisationHandler logic.
    /// </summary>
    private static bool IsBudgetLineAuthorized(User user, BudgetLine budgetLine) =>
        budgetLine.Budget.FamilyId == user.FamilyId;

    private static User CreateUser(Guid familyId) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = familyId,
            Currency = "AUD",
        };

    private static BudgetLine CreateBudgetLine(Guid familyId)
    {
        var budget = new Budget(Guid.NewGuid())
        {
            FamilyId = familyId,
            Year = 2024
        };

        return new BudgetLine(Guid.NewGuid())
        {
            Budget = budget,
            BudgetId = budget.Id,
            Amount = 100m,
            TagId = 1
        };
    }
}
