#nullable enable
using System.Security.Claims;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Core.Tests.Security;

/// <summary>
/// Covers AssertTagPermission against the real handler set, where the tag ids come from a request
/// body and the route belongs to something else entirely.
/// </summary>
/// <remarks>
/// Both tag handlers answer the same requirement, and ASP.NET Core runs every handler registered for
/// it. A route-based handler that vetoes is therefore decisive: context.Fail() outranks any
/// context.Succeed(), so a body check cannot be evaluated in isolation by inspecting one handler.
/// </remarks>
[Trait("Category", "Unit")]
public class TagPermissionCompositionTests
{
    private const int TagId = 42;
    private static readonly Guid _familyId = Guid.NewGuid();

    private static IServiceProvider BuildProvider(Mock<IAuthorisationReader> reader)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped(_ => new User
        {
            Id = Guid.NewGuid(),
            EmailAddress = "test@test.com",
            FamilyId = _familyId,
            Currency = "AUD",
        });
        services.AddScoped(_ => reader.Object);
        services.AddScoped(_ => new Mock<IAuditLogger>().Object);

        var principalProvider = new Mock<IPrincipalProvider>();
        principalProvider.Setup(p => p.Principal).Returns(new ClaimsPrincipal(new ClaimsIdentity("TestAuth")));
        services.AddScoped(_ => principalProvider.Object);

        Asm.MooBank.Security.IServiceCollectionExtensions.AddAuthorisationHandlers(services);

        return services.BuildServiceProvider();
    }

    private static Mock<IAuthorisationReader> ReaderReturning(Guid familyId)
    {
        var reader = new Mock<IAuthorisationReader>();
        reader.Setup(r => r.GetTagFamilyIds(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new Dictionary<int, Guid> { [TagId] = familyId });
        return reader;
    }

    /// <summary>
    /// Given tag ids in the body of a transaction update, whose route carries the transaction's own
    /// id, and the tags belong to the user's family
    /// When AssertTagPermission is called
    /// Then it does not throw
    /// </summary>
    [Fact]
    public async Task AssertTagPermission_RouteIdIsNotATag_TagsInUsersFamily_DoesNotThrow()
    {
        // Arrange
        using var scope = BuildProvider(ReaderReturning(_familyId)).CreateScope();

        // PUT accounts/{instrumentId}/transactions/{id} -- "id" is the transaction, not a tag.
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            CreateHttpContext("id", Guid.NewGuid().ToString());

        var security = scope.ServiceProvider.GetRequiredService<ISecurity>();

        // Act
        var exception = await Record.ExceptionAsync(() => security.AssertTagPermission([TagId]));

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Given tag ids in the body belonging to another family, on that same route
    /// When AssertTagPermission is called
    /// Then it throws
    /// </summary>
    [Fact]
    public async Task AssertTagPermission_RouteIdIsNotATag_TagsInOtherFamily_Throws()
    {
        // Arrange
        using var scope = BuildProvider(ReaderReturning(Guid.NewGuid())).CreateScope();

        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            CreateHttpContext("id", Guid.NewGuid().ToString());

        var security = scope.ServiceProvider.GetRequiredService<ISecurity>();

        // Act
        var exception = await Record.ExceptionAsync(() => security.AssertTagPermission([TagId]));

        // Assert
        Assert.NotNull(exception);
    }

    /// <summary>
    /// Given tag ids in the body and no route values at all
    /// When AssertTagPermission is called
    /// Then it does not throw
    /// </summary>
    [Fact]
    public async Task AssertTagPermission_NoRouteValues_TagsInUsersFamily_DoesNotThrow()
    {
        // Arrange
        using var scope = BuildProvider(ReaderReturning(_familyId)).CreateScope();

        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = CreateHttpContext();

        var security = scope.ServiceProvider.GetRequiredService<ISecurity>();

        // Act
        var exception = await Record.ExceptionAsync(() => security.AssertTagPermission([TagId]));

        // Assert
        Assert.Null(exception);
    }

    private static DefaultHttpContext CreateHttpContext(string? routeParamName = null, object? routeValue = null)
    {
        var httpContext = new DefaultHttpContext();

        if (routeParamName is not null)
        {
            httpContext.Request.RouteValues[routeParamName] = routeValue;
        }

        return httpContext;
    }
}
