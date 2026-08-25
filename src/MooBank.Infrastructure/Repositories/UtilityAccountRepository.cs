using Asm.MooBank.Domain.Entities.Utility;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class UtilityAccountRepository(MooBankContext context, Models.User user) : RepositoryWriteBase<MooBankContext, Account, Guid>(context), IAccountRepository
{
    public override async Task<IEnumerable<Account>> Get(CancellationToken cancellationToken = default) =>
        await Entities
            .Where(a => user.Accounts.Contains(a.Id))
            .Include(a => a.Bills)
            .ToListAsync(cancellationToken);

    public override async Task<Account> Get(Guid id, CancellationToken cancellationToken = default) =>
        await Entities
            .Where(a => a.Id == id && user.Accounts.Contains(a.Id))
            .Include(a => a.Bills)
            .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

    public async Task<Account?> GetWithBill(Guid instrumentId, int billId, CancellationToken cancellationToken = default) =>
        await Entities
            .Where(a => a.Id == instrumentId && user.Accounts.Contains(a.Id))
            // Filtered so that only the bill being edited brings its periods with it.
            .Include(a => a.Bills.Where(b => b.Id == billId)).ThenInclude(b => b.Periods).ThenInclude(p => p.Usages)
            .Include(a => a.Bills.Where(b => b.Id == billId)).ThenInclude(b => b.Periods).ThenInclude(p => p.ServiceCharges)
            .Include(a => a.Bills.Where(b => b.Id == billId)).ThenInclude(b => b.Discounts)
            .SingleOrDefaultAsync(cancellationToken);
}
