using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class RecurringTransactionRepository(MooBankContext context) : RepositoryBase<MooBankContext, RecurringTransaction, Guid>(context), IRecurringTransactionRepository
{
}
