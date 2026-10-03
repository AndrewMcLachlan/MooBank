using Microsoft.AspNetCore.Authorization;

namespace Asm.MooBank.Security.Authorisation;

/// <summary>
/// Requires that a set of tag ids supplied as a resource all belong to the current user's family.
/// </summary>
/// <remarks>
/// Deliberately not a <see cref="Asm.AspNetCore.Authorisation.RouteParamAuthorisationRequirement"/>:
/// every handler registered for a requirement runs, and a route-based handler's veto is decisive,
/// so sharing one with <see cref="TagFamilyRequirement"/> lets an unrelated route value decide a
/// check whose subject is in the request body.
/// </remarks>
public class TagFamilyResourceRequirement : IAuthorizationRequirement
{
}
