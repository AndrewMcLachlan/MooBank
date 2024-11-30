using Asm.AspNetCore.Authorisation;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User? user) : RouteParamAuthorisationHandler<InstrumentOwnerRequirement>(httpContextAccessor)
{    protected override ValueTask<bool> IsAuthorised(object value) =>
        ValueTask.FromResult((value is Guid instrumentId) && user is not null && user.Accounts.Contains(instrumentId));
}
