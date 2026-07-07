using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentViewerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User? user) : InstrumentRouteAuthorisationHandler<InstrumentViewerRequirement>(httpContextAccessor)
{
    protected override bool IsAuthorised(Guid instrumentId) =>
        user is not null && (user.Accounts.Contains(instrumentId) || user.SharedAccounts.Contains(instrumentId));
}
