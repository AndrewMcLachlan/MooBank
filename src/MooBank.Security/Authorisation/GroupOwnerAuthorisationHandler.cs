using Asm.AspNetCore.Authorisation;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class GroupOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User user) : RouteParamAuthorisationHandler<GroupOwnerRequirement>(httpContextAccessor)
{
    protected override ValueTask<bool> IsAuthorised(object value) =>
        ValueTask.FromResult(Guid.TryParse(value.ToString(), out var groupId) && user is not null && user.Groups.Contains(groupId));
}
