using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class RecurringTransactionRepository(MooBankContext context) : RepositoryBase<RecurringTransaction, Guid>(context), IRecurringTransactionRepository
{
    public override async Task<IEnumerable<RecurringTransaction>> Get(CancellationToken cancellationToken = default) =>
        await Entities.Include(rt => rt.VirtualAccount).ToListAsync(cancellationToken);

    protected override IQueryable<RecurringTransaction> GetById(Guid id) => Entities.Where(t => t.Id == id);
}
