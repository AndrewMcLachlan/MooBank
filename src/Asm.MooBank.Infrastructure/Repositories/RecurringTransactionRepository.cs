using Asm.MooBank.Domain.Entities.RecurringTransactions;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class RecurringTransactionRepository : RepositoryBase<RecurringTransaction, int>, IRecurringTransactionRepository
{
    public RecurringTransactionRepository(MooBankContext dataContext) : base(dataContext)
    {
    }

    public override async Task<IEnumerable<RecurringTransaction>> GetAll(CancellationToken cancellationToken = default) =>
        await DataSet.Include(rt => rt.VirtualAccount).ToListAsync(cancellationToken);

    protected override IQueryable<RecurringTransaction> GetById(int id) => DataSet.Where(t => t.RecurringTransactionId == id);
}
