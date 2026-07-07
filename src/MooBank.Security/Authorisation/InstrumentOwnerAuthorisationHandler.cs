using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User? user) : InstrumentRouteAuthorisationHandler<InstrumentOwnerRequirement>(httpContextAccessor)
{
    protected override bool IsAuthorised(Guid instrumentId) =>
        user is not null && user.Accounts.Contains(instrumentId);
}
