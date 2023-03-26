using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.RecurringTransactions;

public interface IRecurringTransactionRepository : IWritableRepository<RecurringTransaction, int>
{

}
