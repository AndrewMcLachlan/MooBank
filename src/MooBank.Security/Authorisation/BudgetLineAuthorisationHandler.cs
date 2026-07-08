using Asm.AspNetCore.Authorisation;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class BudgetLineAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationRepository authorisationRepository, User user, IAuditLogger audit) : RouteParamAuthorisationHandler<BudgetLineRequirement>(httpContextAccessor)
{
    protected override async ValueTask<bool> IsAuthorised(object value)
    {
        if (!Guid.TryParse(value.ToString(), out var id)) return false;

        var permitted = await authorisationRepository.GetBudgetLineFamilyId(id) == user.FamilyId;

        if (!permitted)
        {
            audit.AuthorizationDenied(user, "BudgetLine", id, nameof(BudgetLineRequirement));
        }

        return permitted;
    }
}
