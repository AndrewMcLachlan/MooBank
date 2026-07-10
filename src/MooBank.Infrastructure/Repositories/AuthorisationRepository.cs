using Asm.MooBank.Domain;

namespace Asm.MooBank.Infrastructure.Repositories;

public class AuthorisationRepository(MooBankContext mooBankContext) : IAuthorisationRepository
{
    public async Task<bool> IsGroupOwner(Guid groupId, Guid userId, CancellationToken cancellationToken = default) =>
        await mooBankContext.Groups.AnyAsync(g => g.Id == groupId && g.OwnerId == userId, cancellationToken);

    public async Task<Guid?> GetBudgetLineFamilyId(Guid budgetLineId, CancellationToken cancellationToken = default) =>
        await mooBankContext.BudgetLines.Where(bl => bl.Id == budgetLineId).Select(bl => (Guid?)bl.Budget.FamilyId).SingleOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<Guid>> GetOwnedInstrumentIds(Guid userId, CancellationToken cancellationToken = default) =>
        await mooBankContext.InstrumentOwners.Where(aah => aah.UserId == userId).Select(aah => aah.InstrumentId).ToListAsync(cancellationToken);
}
