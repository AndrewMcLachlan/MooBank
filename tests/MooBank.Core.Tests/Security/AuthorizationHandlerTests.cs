#nullable enable
using System.Security.Claims;
using Asm.MooBank.Core.Tests.Support;
using Asm.MooBank.Models;
using Asm.MooBank.Security.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), user);
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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", UnauthorizedInstrumentId.ToString()), user);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", SharedInstrumentId.ToString()), user);
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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", "not-a-guid"), user);
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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), null);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
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
        var handler = new InstrumentOwnerAuthorisationHandler(CreateHttpContextAccessor(), user);
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
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), user);
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
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", SharedInstrumentId.ToString()), user);
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
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", UnauthorizedInstrumentId.ToString()), user);
        var context = CreateAuthorizationContext(requirement);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
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
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", "invalid"), user);
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
        var handler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor("instrumentId", OwnedInstrumentId.ToString()), null);
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
        var routeHandler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor(), user);
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
        var routeHandler = new InstrumentViewerAuthorisationHandler(CreateHttpContextAccessor(), user);
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

    #region Test Wrappers

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

/// <summary>
/// Tests for the BudgetLineAuthorisationHandler, which authorises the budget line route
/// parameter via ISecurity.HasBudgetLinePermission.
/// </summary>
[Trait("Category", "Unit")]
public class BudgetLineAuthorisationHandlerTests
{
    private static readonly Guid BudgetLineId = Guid.NewGuid();

    /// <summary>
    /// Given the registered authorisation handlers, where ISecurity depends on IAuthorizationService
    /// When IAuthorizationService is resolved
    /// Then no circular dependency should occur, because no handler may depend on ISecurity
    /// </summary>
    [Fact]
    public void BudgetLineHandler_SecurityDependsOnAuthorizationService_NoCircularDependency()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped(_ => new User
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = Guid.NewGuid(),
            Currency = "AUD",
        });
        services.AddScoped<Asm.MooBank.Security.ISecurity, AuthorizationServiceDependentSecurity>();
        services.AddScoped(_ => new Mock<Asm.MooBank.Security.IBudgetLineSecurity>().Object);
        Asm.MooBank.Security.IServiceCollectionExtensions.AddAuthorisationHandlers(services);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        // Act & Assert
        var exception = Record.Exception(() => scope.ServiceProvider.GetRequiredService<IAuthorizationService>());
        Assert.Null(exception);
    }

    private sealed class AuthorizationServiceDependentSecurity(IAuthorizationService authorizationService) : Asm.MooBank.Security.ISecurity
    {
        public Task AssertGroupPermission(Guid groupId) => Task.FromResult(authorizationService is not null);
        public void AssertGroupPermission(Domain.Entities.Group.Group group) => throw new NotImplementedException();
        public Task AssertFamilyPermission(Guid familyId) => throw new NotImplementedException();
        public Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AssertAdministrator(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    /// <summary>
    /// Given a valid budget line id in the route and a user with permission
    /// When the requirement is handled
    /// Then authorization should succeed
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_ValidRouteValueWithPermission_Succeeds()
    {
        // Arrange
        var security = new Mock<Asm.MooBank.Security.IBudgetLineSecurity>();
        security.Setup(s => s.HasBudgetLinePermission(BudgetLineId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new BudgetLineAuthorisationHandler(CreateHttpContextAccessor("id", BudgetLineId.ToString()), security.Object);
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Given a valid budget line id in the route and a user without permission
    /// When the requirement is handled
    /// Then authorization should fail
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_ValidRouteValueWithoutPermission_Fails()
    {
        // Arrange
        var security = new Mock<Asm.MooBank.Security.IBudgetLineSecurity>();
        security.Setup(s => s.HasBudgetLinePermission(BudgetLineId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new BudgetLineAuthorisationHandler(CreateHttpContextAccessor("id", BudgetLineId.ToString()), security.Object);
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    /// <summary>
    /// Given a route value that is not a valid GUID
    /// When the requirement is handled
    /// Then authorization should fail without consulting security
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_InvalidGuidRouteValue_Fails()
    {
        // Arrange
        var security = new Mock<Asm.MooBank.Security.IBudgetLineSecurity>();
        var handler = new BudgetLineAuthorisationHandler(CreateHttpContextAccessor("id", "not-a-guid"), security.Object);
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        security.Verify(s => s.HasBudgetLinePermission(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Given a request without the budget line route value
    /// When the requirement is handled
    /// Then authorization should fail (base class behaviour is fail-closed)
    /// </summary>
    [Fact]
    public async Task BudgetLineHandler_MissingRouteValue_Fails()
    {
        // Arrange
        var security = new Mock<Asm.MooBank.Security.IBudgetLineSecurity>();
        var handler = new BudgetLineAuthorisationHandler(CreateHttpContextAccessor(), security.Object);
        var context = CreateAuthorizationContext(new BudgetLineRequirement());

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        security.Verify(s => s.HasBudgetLinePermission(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
