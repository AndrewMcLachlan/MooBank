using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class ForecastPlanAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationRepository authorisationRepository, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<ForecastPlanRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "ForecastPlan";

    protected override async ValueTask<bool> IsAuthorised(Guid id) =>
        await authorisationRepository.GetForecastPlanFamilyId(id, CancellationToken) == User!.FamilyId;
}
