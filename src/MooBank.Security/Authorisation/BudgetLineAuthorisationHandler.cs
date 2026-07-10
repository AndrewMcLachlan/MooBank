using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class BudgetLineAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationRepository authorisationRepository, User? user, IAuditLogger audit) : TolerantGuidRouteAuthorisationHandler<BudgetLineRequirement>(httpContextAccessor, user, audit)
{
    protected override string ResourceName => "BudgetLine";

    protected override async ValueTask<bool> IsAuthorised(Guid id) =>
        await authorisationRepository.GetBudgetLineFamilyId(id, CancellationToken) == User!.FamilyId;
}
