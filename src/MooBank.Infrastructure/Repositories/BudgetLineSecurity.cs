using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Asm.MooBank.Security;
using Asm.MooBank.Security.Authorisation;

namespace Asm.MooBank.Infrastructure.Repositories;

public class BudgetLineSecurity(MooBankContext mooBankContext, User user, IAuditLogger audit) : IBudgetLineSecurity
{
    public async Task<bool> HasBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var permitted = await mooBankContext.BudgetLines.AnyAsync(bl => bl.Id == id && bl.Budget.FamilyId == user.FamilyId, cancellationToken);

        if (!permitted)
        {
            audit.AuthorizationDenied(user, "BudgetLine", id, nameof(BudgetLineRequirement));
        }

        return permitted;
    }
}
