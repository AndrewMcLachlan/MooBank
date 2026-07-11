#nullable enable
using System.Security.Claims;
using Asm.MooBank.Core.Tests.Support;
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
/// Tests for the instrument route parameter authorization handlers.
/// These invoke the real handlers via HandleAsync with actual route values, verifying
/// that string route values are parsed as GUIDs and that the handlers do not veto
/// resource-based authorization when no route value is present.
/// </summary>
[Trait("Category", "Unit")]
public class RouteParamAuthorizationHandlerTests
{
    private static readonly Guid OwnedInstrumentId = Guid.NewGuid();
    private static readonly Guid SharedInstrumentId = Guid.NewGuid();
    private static readonly Guid UnauthorizedInstrumentId = Guid.NewGuid();
    private static readonly Guid OwnedGroupId = Guid.NewGuid();

    private readonly Mock<Asm.MooBank.Audit.IAuditLogger> _audit = new();

    #region InstrumentOwnerAuthorisationHandler Tests

    /// <summary>
    /// Given a user who owns the instrument in the route
    /// When the owner requirement is handled
    /// Then authorization should succeed (route values are strings and must be parsed as GUIDs)
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_OwnedInstrumentRouteValue_Succeeds()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given a user who does not own the instrument in the route
    /// When the owner requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_NonOwnedInstrumentRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", UnauthorizedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(user, "Instrument", UnauthorizedInstrumentId.ToString(), nameof(InstrumentOwnerRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a user who only has shared access to the instrument in the route
    /// When the owner requirement is handled
    /// Then authorization should fail (shared accounts don't count for owner authorization)
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_SharedInstrumentRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(sharedAccounts: [SharedInstrumentId]);
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", SharedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given a route value that is not a valid GUID
    /// When the owner requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", "not-a-guid"), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given no current user
    /// When the owner requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_NullUser_Fails()
    {
        // Arrange
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), null, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given a request without an instrument route value (e.g. a resource-based check on /mcp)
    /// When the owner requirement is handled
    /// Then the handler should neither succeed nor fail, leaving the decision to resource-based handlers
    /// </summary>
    [Fact]
    public async Task InstrumentOwnerHandler_NoRouteValue_DoesNotVeto()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentOwnerRequirement();
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor(), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    #endregion

    #region InstrumentViewerAuthorisationHandler Tests

    /// <summary>
    /// Given a user who owns the instrument in the route
    /// When the viewer requirement is handled
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_OwnedInstrumentRouteValue_Succeeds()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given a user who has shared access to the instrument in the route
    /// When the viewer requirement is handled
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_SharedInstrumentRouteValue_Succeeds()
    {
        // Arrange
        var user = CreateUser(sharedAccounts: [SharedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", SharedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given a user with no access to the instrument in the route
    /// When the viewer requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_UnauthorizedInstrumentRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", UnauthorizedInstrumentId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(user, "Instrument", UnauthorizedInstrumentId.ToString(), nameof(InstrumentViewerRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a route value that is not a valid GUID
    /// When the viewer requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", "invalid"), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given no current user
    /// When the viewer requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_NullUser_Fails()
    {
        // Arrange
        var requirement = new InstrumentViewerRequirement();
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), null, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given a request without an instrument route value (e.g. a resource-based check on /mcp)
    /// When the viewer requirement is handled by both the route handler and the resource handler
    /// Then the resource handler's success should not be vetoed by the route handler
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_NoRouteValue_ResourceHandlerDecides()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var routeHandler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor(), user, _audit.Object);
        var resourceHandler = new InstrumentViewerResourceAuthorisationHandler(user);
        var context = CreateAuthorizationContext(requirement, OwnedInstrumentId);

        // Act
        await routeHandler.HandleAsync(context);
        await resourceHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given a request without an instrument route value and a user without access to the resource
    /// When the viewer requirement is handled by both the route handler and the resource handler
    /// Then authorization should not succeed (fail-closed)
    /// </summary>
    [Fact]
    public async Task InstrumentViewerHandler_NoRouteValue_UnauthorizedResource_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(accounts: [OwnedInstrumentId]);
        var requirement = new InstrumentViewerRequirement();
        var routeHandler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor(), user, _audit.Object);
        var resourceHandler = new InstrumentViewerResourceAuthorisationHandler(user);
        var context = CreateAuthorizationContext(requirement, UnauthorizedInstrumentId);

        // Act
        await routeHandler.HandleAsync(context);
        await resourceHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    #endregion

    #region GroupOwnerAuthorisationHandler Tests

    [Fact]
    public async Task GroupOwnerHandler_OwnedGroupRouteValue_Succeeds()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var requirement = new GroupOwnerRequirement();
        var handler = new GroupOwnerAuthorisationHandler(CreateHttpContextAccessor("groupId", OwnedGroupId.ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task GroupOwnerHandler_NonOwnedGroupRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var requirement = new GroupOwnerRequirement();
        var handler = new GroupOwnerAuthorisationHandler(CreateHttpContextAccessor("groupId", Guid.NewGuid().ToString()), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task GroupOwnerHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var requirement = new GroupOwnerRequirement();
        var handler = new GroupOwnerAuthorisationHandler(CreateHttpContextAccessor("groupId", "not-a-guid"), user, _audit.Object);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given no group route value and a resource-based evaluation for an owned group
    /// When both group handlers run
    /// Then the resource handler decides and authorization succeeds
    /// </summary>
    [Fact]
    public async Task GroupOwnerHandlers_NoRouteValue_OwnedGroupResource_Succeeds()
    {
        // Arrange
        var user = CreateUser(groups: [OwnedGroupId]);
        var repository = new Mock<Asm.MooBank.Domain.IAuthorisationReader>();
        repository.Setup(r => r.IsGroupOwner(OwnedGroupId, user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var requirement = new GroupOwnerRequirement();
        var routeHandler = new GroupOwnerAuthorisationHandler(CreateHttpContextAccessor(), user, _audit.Object);
        var resourceHandler = new GroupOwnerResourceAuthorisationHandler(repository.Object, user);
        var context = CreateAuthorizationContext(requirement, OwnedGroupId);

        // Act
        await routeHandler.HandleAsync(context);
        await resourceHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given no group route value and a resource-based evaluation for a group the user does not own
    /// When both group handlers run
    /// Then authorization does not succeed (fail-closed)
    /// </summary>
    [Fact]
    public async Task GroupOwnerHandlers_NoRouteValue_NonOwnedGroupResource_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser();
        var repository = new Mock<Asm.MooBank.Domain.IAuthorisationReader>();
        repository.Setup(r => r.IsGroupOwner(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var requirement = new GroupOwnerRequirement();
        var routeHandler = new GroupOwnerAuthorisationHandler(CreateHttpContextAccessor(), user, _audit.Object);
        var resourceHandler = new GroupOwnerResourceAuthorisationHandler(repository.Object, user);
        var context = CreateAuthorizationContext(requirement, Guid.NewGuid());

        // Act
        await routeHandler.HandleAsync(context);
        await resourceHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    #endregion

    #region Helpers

    private static AuthorizationHandlerContext CreateAuthorizationContext(
        IAuthorizationRequirement requirement,
        object? resource = null)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, resource);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? routeParamName = null, object? routeValue = null)
    {
        var httpContext = new DefaultHttpContext();

        if (routeParamName is not null)
        {
            httpContext.Request.RouteValues[routeParamName] = routeValue;
        }

        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
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

}

/// <summary>
/// Tests for the BudgetLineAuthorisationHandler, which authorises the budget line route
/// parameter via IAuthorisationReader.
/// </summary>
[Trait("Category", "Unit")]
public class BudgetLineAuthorisationHandlerTests
{
    private static readonly Guid BudgetLineId = Guid.NewGuid();
    private static readonly Guid FamilyId = Guid.NewGuid();

    private readonly Mock<Asm.MooBank.Domain.IAuthorisationReader> _repository = new();
    private readonly Mock<Asm.MooBank.Audit.IAuditLogger> _audit = new();
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = FamilyId,
        Currency = "AUD",
    };

    private BudgetLineAuthorisationHandler CreateHandler(IHttpContextAccessor httpContextAccessor) =>
        new(httpContextAccessor, _repository.Object, _user, _audit.Object);

    /// <summary>
    /// Given a valid budget line id in the route belonging to the user's family
    /// When the requirement is handled
    /// Then authorization should succeed and nothing is audited
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_ValidRouteValueInUsersFamily_Succeeds()
    {
        // Arrange
        _repository.Setup(r => r.GetBudgetLineFamilyId(BudgetLineId, It.IsAny<CancellationToken>())).ReturnsAsync(FamilyId);
        var handler = CreateHandler(CreateHttpContextAccessor("id", BudgetLineId.ToString()));
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given a valid budget line id in the route belonging to another family
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_ValidRouteValueInOtherFamily_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetBudgetLineFamilyId(BudgetLineId, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        var handler = CreateHandler(CreateHttpContextAccessor("id", BudgetLineId.ToString()));
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "BudgetLine", BudgetLineId.ToString(), nameof(BudgetLineRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a budget line id that does not exist
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_UnknownBudgetLine_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetBudgetLineFamilyId(BudgetLineId, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        var handler = CreateHandler(CreateHttpContextAccessor("id", BudgetLineId.ToString()));
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "BudgetLine", BudgetLineId.ToString(), nameof(BudgetLineRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a route value that is not a valid GUID
    /// When the requirement is handled
    /// Then authorization should fail without consulting the repository
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor("id", "not-a-guid"));
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _repository.Verify(r => r.GetBudgetLineFamilyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a request without the budget line route value
    /// When the requirement is handled
    /// Then the handler should neither succeed nor fail (tolerant behaviour)
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_MissingRouteValue_DoesNotVeto()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor());
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _repository.Verify(r => r.GetBudgetLineFamilyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(IAuthorizationRequirement requirement)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? routeParamName = null, object? routeValue = null)
    {
        var httpContext = new DefaultHttpContext();

        if (routeParamName is not null)
        {
            httpContext.Request.RouteValues[routeParamName] = routeValue;
        }

        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock.Object;
    }
}

/// <summary>
/// Tests for the TagFamilyAuthorisationHandler, which authorises the tag route
/// parameter via IAuthorisationReader.
/// </summary>
[Trait("Category", "Unit")]
public class TagFamilyAuthorisationHandlerTests
{
    private const int TagId = 42;
    private static readonly Guid FamilyId = Guid.NewGuid();

    private readonly Mock<Asm.MooBank.Domain.IAuthorisationReader> _repository = new();
    private readonly Mock<Asm.MooBank.Audit.IAuditLogger> _audit = new();
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = FamilyId,
        Currency = "AUD",
    };

    private TagFamilyAuthorisationHandler CreateHandler(IHttpContextAccessor httpContextAccessor) =>
        new(httpContextAccessor, _repository.Object, _user, _audit.Object);

    /// <summary>
    /// Given a valid tag id in the route belonging to the user's family
    /// When the requirement is handled
    /// Then authorization should succeed and nothing is audited
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_ValidRouteValueInUsersFamily_Succeeds()
    {
        // Arrange
        _repository.Setup(r => r.GetTagFamilyId(TagId, It.IsAny<CancellationToken>())).ReturnsAsync(FamilyId);
        var handler = CreateHandler(CreateHttpContextAccessor("id", TagId.ToString()));
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given a valid tag id in the route belonging to another family
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_ValidRouteValueInOtherFamily_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetTagFamilyId(TagId, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        var handler = CreateHandler(CreateHttpContextAccessor("id", TagId.ToString()));
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "Tag", TagId.ToString(), nameof(TagFamilyRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a tag id that does not exist
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_UnknownTag_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetTagFamilyId(TagId, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        var handler = CreateHandler(CreateHttpContextAccessor("id", TagId.ToString()));
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "Tag", TagId.ToString(), nameof(TagFamilyRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a route value that is not a valid integer
    /// When the requirement is handled
    /// Then authorization should fail without consulting the repository
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_InvalidIntRouteValue_Fails()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor("id", "not-an-int"));
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _repository.Verify(r => r.GetTagFamilyId(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a request without the tag route value
    /// When the requirement is handled
    /// Then the handler should neither succeed nor fail (tolerant behaviour)
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_MissingRouteValue_DoesNotVeto()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor());
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _repository.Verify(r => r.GetTagFamilyId(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given no current user
    /// When the requirement is handled
    /// Then authorization should fail and nothing is audited
    /// </summary>
    [Fact]
    public async Task TagFamilyHandler_NullUser_FailsWithoutAudit()
    {
        // Arrange
        var handler = new TagFamilyAuthorisationHandler(CreateHttpContextAccessor("id", TagId.ToString()), _repository.Object, null, _audit.Object);
        var context = CreateAuthorizationContext(new TagFamilyRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(IAuthorizationRequirement requirement)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? routeParamName = null, object? routeValue = null)
    {
        var httpContext = new DefaultHttpContext();

        if (routeParamName is not null)
        {
            httpContext.Request.RouteValues[routeParamName] = routeValue;
        }

        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock.Object;
    }
}

/// <summary>
/// Tests for the ForecastPlanAuthorisationHandler, which authorises the forecast plan route
/// parameter via IAuthorisationReader.
/// </summary>
[Trait("Category", "Unit")]
public class ForecastPlanAuthorisationHandlerTests
{
    private static readonly Guid PlanId = Guid.NewGuid();
    private static readonly Guid FamilyId = Guid.NewGuid();

    private readonly Mock<Asm.MooBank.Domain.IAuthorisationReader> _repository = new();
    private readonly Mock<Asm.MooBank.Audit.IAuditLogger> _audit = new();
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = FamilyId,
        Currency = "AUD",
    };

    private ForecastPlanAuthorisationHandler CreateHandler(IHttpContextAccessor httpContextAccessor) =>
        new(httpContextAccessor, _repository.Object, _user, _audit.Object);

    /// <summary>
    /// Given a valid plan id in the route belonging to the user's family
    /// When the requirement is handled
    /// Then authorization should succeed and nothing is audited
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_ValidRouteValueInUsersFamily_Succeeds()
    {
        // Arrange
        _repository.Setup(r => r.GetForecastPlanFamilyId(PlanId, It.IsAny<CancellationToken>())).ReturnsAsync(FamilyId);
        var handler = CreateHandler(CreateHttpContextAccessor("id", PlanId.ToString()));
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given a valid plan id in the route belonging to another family
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_ValidRouteValueInOtherFamily_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetForecastPlanFamilyId(PlanId, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        var handler = CreateHandler(CreateHttpContextAccessor("id", PlanId.ToString()));
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "ForecastPlan", PlanId.ToString(), nameof(ForecastPlanRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a plan id that does not exist
    /// When the requirement is handled
    /// Then authorization should fail and the denial is audited
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_UnknownPlan_FailsAndAudits()
    {
        // Arrange
        _repository.Setup(r => r.GetForecastPlanFamilyId(PlanId, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);
        var handler = CreateHandler(CreateHttpContextAccessor("id", PlanId.ToString()));
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(_user, "ForecastPlan", PlanId.ToString(), nameof(ForecastPlanRequirement)), Times.Once);
    }

    /// <summary>
    /// Given a route value that is not a valid GUID
    /// When the requirement is handled
    /// Then authorization should fail without consulting the repository
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor("id", "not-a-guid"));
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _repository.Verify(r => r.GetForecastPlanFamilyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a request without the plan route value
    /// When the requirement is handled
    /// Then the handler should neither succeed nor fail (tolerant behaviour)
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_MissingRouteValue_DoesNotVeto()
    {
        // Arrange
        var handler = CreateHandler(CreateHttpContextAccessor());
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _repository.Verify(r => r.GetForecastPlanFamilyId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given no current user
    /// When the requirement is handled
    /// Then authorization should fail and nothing is audited
    /// </summary>
    [Fact]
    public async Task ForecastPlanHandler_NullUser_FailsWithoutAudit()
    {
        // Arrange
        var handler = new ForecastPlanAuthorisationHandler(CreateHttpContextAccessor("id", PlanId.ToString()), _repository.Object, null, _audit.Object);
        var context = CreateAuthorizationContext(new ForecastPlanRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(IAuthorizationRequirement requirement)
    {
        var requirements = new[] { requirement };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? routeParamName = null, object? routeValue = null)
    {
        var httpContext = new DefaultHttpContext();

        if (routeParamName is not null)
        {
            httpContext.Request.RouteValues[routeParamName] = routeValue;
        }

        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        return mock.Object;
    }
}
/// <summary>
/// Tests for the AdminAuthorisationHandler, which backs the Admin policy so that
/// denials are audited.
/// </summary>
[Trait("Category", "Unit")]
public class AdminAuthorisationHandlerTests
{
    private readonly Mock<Asm.MooBank.Audit.IAuditLogger> _audit = new();
    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        EmailAddress = "test@test.com",
        FamilyId = Guid.NewGuid(),
        Currency = "AUD",
    };

    /// <summary>
    /// Given a principal in the Admin role
    /// When the requirement is handled
    /// Then authorization should succeed and nothing is audited
    /// </summary>
    [Fact]
    public async Task AdminHandler_UserInAdminRole_Succeeds()
    {
        // Arrange
        var handler = new RoleAuthorisationHandler(_user, _audit.Object);
        var context = CreateAuthorizationContext(new AdminRequirement(), isAdmin: true);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Given a principal not in the Admin role
    /// When the requirement is handled
    /// Then authorization should not succeed and the denial is audited
    /// </summary>
    [Fact]
    public async Task AdminHandler_UserNotInAdminRole_DoesNotSucceedAndAudits()
    {
        // Arrange
        var handler = new RoleAuthorisationHandler(_user, _audit.Object);
        var context = CreateAuthorizationContext(new AdminRequirement(), isAdmin: false);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        _audit.Verify(a => a.AuthorizationDenied(_user, "Administrator", null, nameof(RoleRequirement)), Times.Once);
    }

    /// <summary>
    /// Given no current user
    /// When the requirement is handled
    /// Then authorization should not succeed and nothing is audited
    /// </summary>
    [Fact]
    public async Task AdminHandler_NullUser_DoesNotSucceedAndDoesNotAudit()
    {
        // Arrange
        var handler = new RoleAuthorisationHandler(null, _audit.Object);
        var context = CreateAuthorizationContext(new AdminRequirement(), isAdmin: false);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        _audit.Verify(a => a.AuthorizationDenied(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<string>()), Times.Never);
    }

    private static AuthorizationHandlerContext CreateAuthorizationContext(IAuthorizationRequirement requirement, bool isAdmin)
    {
        var requirements = new[] { requirement };
        Claim[] claims = isAdmin ? [new Claim(ClaimTypes.Role, AdminRequirement.AdminRoleName)] : [];
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
    }
}
