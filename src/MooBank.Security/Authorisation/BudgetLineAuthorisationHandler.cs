using Asm.AspNetCore.Authorisation;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain;
using Asm.MooBank.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class BudgetLineAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAuthorisationRepository authorisationRepository, User user, IAuditLogger audit) : RouteParamAuthorisationHandler<BudgetLineRequirement>(httpContextAccessor)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async ValueTask<bool> IsAuthorised(object value)
    {
        if (!Guid.TryParse(value.ToString(), out var id)) return false;

        var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
        var permitted = await authorisationRepository.GetBudgetLineFamilyId(id, cancellationToken) == user.FamilyId;

        if (!permitted)
        {
            audit.AuthorizationDenied(user, "BudgetLine", id, nameof(BudgetLineRequirement));
        }

        return permitted;
    }
}
