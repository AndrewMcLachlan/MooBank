using Asm.AspNetCore.Authorisation;
using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Base handler for authorisation based on a route parameter.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="RouteParamAuthorisationHandler{TRequirement}"/>, this handler does not veto the
/// authorisation result when the route value is absent. This allows resource-based authorisation
/// (e.g. <see cref="ISecurity.AssertInstrumentViewer"/> on non-instrument routes such as /mcp)
/// to be decided by the resource-based handlers. When no handler succeeds, the requirement still fails,
/// so authorisation remains fail-closed. A route value that is present but invalid or unauthorised
/// fails the requirement outright.
/// </para>
/// <para>
/// Denials are audited here so that every route-policy denial is logged uniformly.
/// Resource-based handlers do not audit; their callers (<see cref="ISecurity"/>) do.
/// </para>
/// </remarks>
internal abstract class TolerantRouteAuthorisationHandler<TRequirement>(IHttpContextAccessor httpContextAccessor, User? user, IAuditLogger audit) : AuthorizationHandler<TRequirement>
    where TRequirement : RouteParamAuthorisationRequirement
{
    /// <summary>
    /// The name of the resource being authorised, used for audit logging.
    /// </summary>
    protected abstract string ResourceName { get; }

    /// <summary>
    /// The current user, if authenticated.
    /// </summary>
    protected User? User => user;

    /// <summary>
    /// The cancellation token for the current request.
    /// </summary>
    protected CancellationToken CancellationToken => httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        object? value = null;

        if (httpContextAccessor.HttpContext?.Request.RouteValues.TryGetValue(requirement.Name, out value) != true || value is null)
        {
            return;
        }

        if (user is not null && await IsAuthorised(value))
        {
            context.Succeed(requirement);
        }
        else
        {
            if (user is not null)
            {
                audit.AuthorizationDenied(user, ResourceName, value, typeof(TRequirement).Name);
            }

            context.Fail();
        }
    }

    /// <summary>
    /// Performs authorisation based on the route parameter value. Only called when a user is authenticated.
    /// </summary>
    protected abstract ValueTask<bool> IsAuthorised(object value);
}

/// <summary>
/// Base handler for authorisation based on a GUID route parameter.
/// </summary>
/// <remarks>Inherits the tolerant, audited behaviour of <see cref="TolerantRouteAuthorisationHandler{TRequirement}"/>.</remarks>
internal abstract class TolerantGuidRouteAuthorisationHandler<TRequirement>(IHttpContextAccessor httpContextAccessor, User? user, IAuditLogger audit) : TolerantRouteAuthorisationHandler<TRequirement>(httpContextAccessor, user, audit)
    where TRequirement : RouteParamAuthorisationRequirement
{
    protected override async ValueTask<bool> IsAuthorised(object value) =>
        Guid.TryParse(value.ToString(), out var id) && await IsAuthorised(id);

    /// <summary>
    /// Performs authorisation based on the ID from the route.
    /// </summary>
    protected abstract ValueTask<bool> IsAuthorised(Guid id);
}
