using Asm.AspNetCore.Authorisation;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Security.Authorisation;

internal class BudgetLineAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IBudgetLineSecurity security) : RouteParamAuthorisationHandler<BudgetLineRequirement>(httpContextAccessor)
{
    protected override async ValueTask<bool> IsAuthorised(object value) =>
        Guid.TryParse(value.ToString(), out var id) && await security.HasBudgetLinePermission(id);
}
