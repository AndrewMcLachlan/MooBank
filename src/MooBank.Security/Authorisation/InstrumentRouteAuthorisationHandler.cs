using Asm.AspNetCore.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Base handler for instrument authorisation based on a route parameter.
/// </summary>
/// <remarks>
/// Unlike <see cref="RouteParamAuthorisationHandler{TRequirement}"/>, this handler does not veto the
/// authorisation result when the route value is absent. This allows resource-based authorisation
/// (e.g. <see cref="AuthorisationExtensions.AssertInstrumentViewer"/> on non-instrument routes such as /mcp)
/// to be decided by the resource-based handlers. When no handler succeeds, the requirement still fails,
/// so authorisation remains fail-closed. A route value that is present but invalid or unauthorised
/// fails the requirement outright.
/// </remarks>
internal abstract class InstrumentRouteAuthorisationHandler<TRequirement>(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<TRequirement>
    where TRequirement : RouteParamAuthorisationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        object? value = null;

        if (httpContextAccessor.HttpContext?.Request.RouteValues.TryGetValue(requirement.Name, out value) != true || value is null)
        {
            return Task.CompletedTask;
        }

        if (Guid.TryParse(value.ToString(), out var instrumentId) && IsAuthorised(instrumentId))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs authorisation based on the instrument ID from the route.
    /// </summary>
    protected abstract bool IsAuthorised(Guid instrumentId);
}
