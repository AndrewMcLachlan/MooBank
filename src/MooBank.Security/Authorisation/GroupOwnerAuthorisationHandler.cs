using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class GroupOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<GroupOwnerRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "Group";

    protected override ValueTask<bool> IsAuthorised(Guid id) =>
        ValueTask.FromResult(User!.Groups.Contains(id));
}
