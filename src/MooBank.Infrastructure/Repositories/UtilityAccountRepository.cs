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
}
