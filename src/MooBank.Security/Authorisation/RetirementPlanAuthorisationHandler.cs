using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class RetirementPlanAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationReader authorisationReader, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<RetirementPlanRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "RetirementPlan";

    protected override async ValueTask<bool> IsAuthorised(Guid id) =>
        await authorisationReader.GetRetirementPlanFamilyId(id, CancellationToken) == User!.FamilyId;
}
