using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface ITransactionTagRuleRepository : IDeletableRepository<TransactionTagRule, int>
{
    Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default);
    Task<TransactionTagRule> Get(Guid accountId, int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransactionTagRule>> GetForAccount(Guid accountId, CancellationToken cancellationToken = default);
}
