using Asm.MooBank.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public class AuthorisationReader(MooBankContext mooBankContext) : IAuthorisationReader
{
    public async Task<bool> IsGroupOwner(Guid groupId, Guid userId, CancellationToken cancellationToken = default) =>
        await mooBankContext.Groups.AnyAsync(g => g.Id == groupId && g.OwnerId == userId, cancellationToken);

    public async Task<Guid?> GetBudgetLineFamilyId(Guid budgetLineId, CancellationToken cancellationToken = default) =>
        await mooBankContext.BudgetLines.Where(bl => bl.Id == budgetLineId).Select(bl => (Guid?)bl.Budget.FamilyId).SingleOrDefaultAsync(cancellationToken);

    public async Task<Guid?> GetTagFamilyId(int tagId, CancellationToken cancellationToken = default) =>
        await mooBankContext.Set<Domain.Entities.Tag.Tag>().IgnoreQueryFilters().Where(t => t.Id == tagId).Select(t => (Guid?)t.FamilyId).SingleOrDefaultAsync(cancellationToken);

    public async Task<Guid?> GetForecastPlanFamilyId(Guid planId, CancellationToken cancellationToken = default) =>
        await mooBankContext.ForecastPlans.Where(p => p.Id == planId).Select(p => (Guid?)p.FamilyId).SingleOrDefaultAsync(cancellationToken);
}
