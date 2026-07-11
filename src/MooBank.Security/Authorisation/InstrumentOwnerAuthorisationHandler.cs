using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class InstrumentOwnerAuthorisationHandler(IHttpContextAccessor httpContextAccessor, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<InstrumentOwnerRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "Instrument";

    protected override ValueTask<bool> IsAuthorised(Guid id) =>
        ValueTask.FromResult(User!.Accounts.Contains(id));
}
