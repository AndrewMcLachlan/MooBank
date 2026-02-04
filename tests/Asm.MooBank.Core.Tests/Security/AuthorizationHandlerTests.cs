#nullable enable
using System.Security.Claims;
using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models;
using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Tests for the actual FamilyMemberAuthorisationHandler class.
/// These tests invoke the real handler code rather than replicating the logic.
/// </summary>
[Trait("Category", "Unit")]
public class FamilyMemberAuthorisationHandlerTests
{
    private static readonly Guid TestFamilyId = Guid.NewGuid();
    private static readonly Guid OtherFamilyId = Guid.NewGuid();

    [Fact]
    public async Task HandleRequirementAsync_UserInSameFamily_Succeeds()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new FamilyMemberAuthorisationHandler(user);
        var requirement = new FamilyMemberRequirement();
        var context = CreateAuthorizationContext(requirement, TestFamilyId);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_UserInDifferentFamily_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new FamilyMemberAuthorisationHandler(user);
        var requirement = new FamilyMemberRequirement();
        var context = CreateAuthorizationContext(requirement, OtherFamilyId);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_EmptyFamilyId_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new FamilyMemberAuthorisationHandler(user);
        var requirement = new FamilyMemberRequirement();
        var context = CreateAuthorizationContext(requirement, Guid.Empty);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(
        IAuthorizationRequirement requirement,
        Guid resource)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, resource);
    }

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
/// Tests for the actual BudgetLineAuthorisationHandler class.
/// These tests invoke the real handler code rather than replicating the logic.
/// </summary>
[Trait("Category", "Unit")]
public class BudgetLineAuthorisationHandlerTests
{
    private static readonly Guid TestFamilyId = Guid.NewGuid();
    private static readonly Guid OtherFamilyId = Guid.NewGuid();

    [Fact]
    public async Task HandleRequirementAsync_BudgetLineInUserFamily_Succeeds()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new BudgetLineAuthorisationHandler(user);
        var requirement = new BudgetLineRequirement();
        var budgetLine = CreateBudgetLine(TestFamilyId);
        var context = CreateAuthorizationContext(requirement, budgetLine);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_BudgetLineInDifferentFamily_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new BudgetLineAuthorisationHandler(user);
        var requirement = new BudgetLineRequirement();
        var budgetLine = CreateBudgetLine(OtherFamilyId);
        var context = CreateAuthorizationContext(requirement, budgetLine);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleBudgetLinesInSameFamily_AllSucceed()
    {
        // Arrange
        var user = CreateUser(TestFamilyId);
        var handler = new BudgetLineAuthorisationHandler(user);
        var requirement = new BudgetLineRequirement();

        var budgetLine1 = CreateBudgetLine(TestFamilyId);
        var budgetLine2 = CreateBudgetLine(TestFamilyId);

        var context1 = CreateAuthorizationContext(requirement, budgetLine1);
        var context2 = CreateAuthorizationContext(requirement, budgetLine2);

        // Act
        await handler.HandleAsync(context1);
        await handler.HandleAsync(context2);

        // Assert
        Assert.True(context1.HasSucceeded);
        Assert.True(context2.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(
        IAuthorizationRequirement requirement,
        BudgetLine resource)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, resource);
    }

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
            Year = 2024,
        };

        return new BudgetLine(Guid.NewGuid())
        {
            Budget = budget,
            BudgetId = budget.Id,
            Amount = 100m,
            TagId = 1,
        };
    }
}

/// <summary>
/// Tests for InstrumentIdRequirement and its derived classes.
/// These verify the requirement classes can be constructed.
/// </summary>
[Trait("Category", "Unit")]
public class InstrumentRequirementTests
{
    [Fact]
    public void InstrumentIdRequirement_DefaultConstructor_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentIdRequirement();

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void InstrumentIdRequirement_CustomId_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentIdRequirement("customId");

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void InstrumentOwnerRequirement_DefaultConstructor_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentOwnerRequirement();

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void InstrumentOwnerRequirement_CustomId_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentOwnerRequirement("accountId");

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void InstrumentViewerRequirement_DefaultConstructor_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentViewerRequirement();

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void InstrumentViewerRequirement_CustomId_CreatesInstance()
    {
        // Act
        var requirement = new InstrumentViewerRequirement("viewId");

        // Assert
        Assert.NotNull(requirement);
    }
}

/// <summary>
/// Tests for GroupOwnerRequirement.
/// </summary>
[Trait("Category", "Unit")]
public class GroupOwnerRequirementTests
{
    [Fact]
    public void GroupOwnerRequirement_DefaultConstructor_CreatesInstance()
    {
        // Act
        var requirement = new GroupOwnerRequirement();

        // Assert
        Assert.NotNull(requirement);
    }

    [Fact]
    public void GroupOwnerRequirement_CustomId_CreatesInstance()
    {
        // Act
        var requirement = new GroupOwnerRequirement("customGroupId");

        // Assert
        Assert.NotNull(requirement);
    }
}

/// <summary>
/// Tests that invoke actual route param authorization handlers via test wrappers.
/// These test the IsAuthorised method logic directly.
/// </summary>
[Trait("Category", "Unit")]
public class RouteParamAuthorizationHandlerTests
{
    private static readonly Guid OwnedInstrumentId = Guid.NewGuid();
    private static readonly Guid SharedInstrumentId = Guid.NewGuid();
    private static readonly Guid UnauthorizedInstrumentId = Guid.NewGuid();
    private static readonly Guid OwnedGroupId = Guid.NewGuid();

    #region InstrumentOwnerAuthorisationHandler Tests

    [Fact]
    public async Task InstrumentOwnerHandler_OwnedInstrument_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedInstrumentId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InstrumentOwnerHandler_NonOwnedInstrument_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(UnauthorizedInstrumentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstrumentOwnerHandler_SharedInstrument_ReturnsFalse()
    {
        // Arrange - shared accounts don't count for owner authorization
        var user = CreateUser(sharedAccounts: [SharedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(SharedInstrumentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstrumentOwnerHandler_InvalidGuidString_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised("not-a-guid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstrumentOwnerHandler_NullUser_ReturnsFalse()
    {
        // Arrange
        User? user = null;
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedInstrumentId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region InstrumentViewerAuthorisationHandler Tests

    [Fact]
    public async Task InstrumentViewerHandler_OwnedInstrument_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentViewerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedInstrumentId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InstrumentViewerHandler_SharedInstrument_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser(sharedAccounts: [SharedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentViewerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(SharedInstrumentId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task InstrumentViewerHandler_UnauthorizedInstrument_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentViewerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(UnauthorizedInstrumentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstrumentViewerHandler_InvalidGuidString_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentViewerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised("invalid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task InstrumentViewerHandler_NullUser_ReturnsFalse()
    {
        // Arrange
        User? user = null;
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableInstrumentViewerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedInstrumentId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GroupOwnerAuthorisationHandler Tests

    [Fact]
    public async Task GroupOwnerHandler_OwnedGroup_ReturnsTrue()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableGroupOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedGroupId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GroupOwnerHandler_NonOwnedGroup_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableGroupOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GroupOwnerHandler_InvalidGuidString_ReturnsFalse()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableGroupOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised("not-a-guid");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GroupOwnerHandler_NullUser_ReturnsFalse()
    {
        // Arrange
        User? user = null;
        var httpContextAccessor = CreateHttpContextAccessor();
        var handler = new TestableGroupOwnerHandler(httpContextAccessor, user);

        // Act
        var result = await handler.TestIsAuthorised(OwnedGroupId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Helpers

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        return mock.Object;
    }

    private static User CreateUser(
        Guid[]? accounts = null,
        Guid[]? sharedAccounts = null,
        Guid[]? groups = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
            Accounts = accounts ?? [],
            SharedAccounts = sharedAccounts ?? [],
            Groups = groups ?? [],
        };

    #endregion

    #region Test Wrappers

    /// <summary>
    /// Test wrapper that exposes the protected IsAuthorised method.
    /// </summary>
    private class TestableInstrumentOwnerHandler : InstrumentOwnerAuthorisationHandler
    {
        public TestableInstrumentOwnerHandler(IHttpContextAccessor httpContextAccessor, User? user)
            : base(httpContextAccessor, user!)
        {
        }

        public ValueTask<bool> TestIsAuthorised(object value) => IsAuthorised(value);
    }

    /// <summary>
    /// Test wrapper that exposes the protected IsAuthorised method.
    /// </summary>
    private class TestableInstrumentViewerHandler : InstrumentViewerAuthorisationHandler
    {
        public TestableInstrumentViewerHandler(IHttpContextAccessor httpContextAccessor, User? user)
            : base(httpContextAccessor, user!)
        {
        }

        public ValueTask<bool> TestIsAuthorised(object value) => IsAuthorised(value);
    }

    /// <summary>
    /// Test wrapper that exposes the protected IsAuthorised method.
    /// </summary>
    private class TestableGroupOwnerHandler : GroupOwnerAuthorisationHandler
    {
        public TestableGroupOwnerHandler(IHttpContextAccessor httpContextAccessor, User? user)
            : base(httpContextAccessor, user!)
        {
        }

        public ValueTask<bool> TestIsAuthorised(object value) => IsAuthorised(value);
    }

    #endregion
}
