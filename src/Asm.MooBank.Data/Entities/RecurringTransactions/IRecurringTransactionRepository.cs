using Asm.MooBank.Domain.Entities.RecurringTransactions;

namespace Asm.MooBank.Domain.Repositories;

public interface IRecurringTransactionRepository : IWritableRepository<RecurringTransaction, int>
{

}
