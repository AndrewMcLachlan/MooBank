using Asm.AspNetCore.Authorisation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Asm.MooBank.Security.Authorisation;

// ISecurity is resolved per evaluation rather than constructor-injected: resolving
// IAuthorizationService constructs every registered handler, and ISecurity's implementation
// depends on IAuthorizationService, so constructor injection creates a circular dependency.
internal class BudgetLineAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) : RouteParamAuthorisationHandler<BudgetLineRequirement>(httpContextAccessor)
{
    protected override async ValueTask<bool> IsAuthorised(object value) =>
        Guid.TryParse(value.ToString(), out var id) && await serviceProvider.GetRequiredService<ISecurity>().HasBudgetLinePermission(id);
}
