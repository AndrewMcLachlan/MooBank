using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class ForecastPlanAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationReader authorisationReader, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<ForecastPlanRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "ForecastPlan";

    protected override async ValueTask<bool> IsAuthorised(Guid id) =>
        await authorisationReader.GetForecastPlanFamilyId(id, CancellationToken) == User!.FamilyId;
}
