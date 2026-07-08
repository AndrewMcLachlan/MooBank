using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class GroupOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User user) : InstrumentRouteAuthorisationHandler<GroupOwnerRequirement>(httpContextAccessor)
{
    protected override bool IsAuthorised(Guid groupId) => user is not null && user.Groups.Contains(groupId);
}
