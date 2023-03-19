using Asm.MooBank.Domain.Entities.RecurringTransactions;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class RecurringTransactionRepository : RepositoryBase<RecurringTransaction, int>, IRecurringTransactionRepository
{
    public RecurringTransactionRepository(BankPlusContext dataContext) : base(dataContext)
    {
    }

    protected override IQueryable<RecurringTransaction> GetById(int id) => DataSet.Where(t => t.RecurringTransactionId == id);
}
