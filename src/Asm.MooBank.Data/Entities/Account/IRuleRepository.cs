using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IRuleRepository : IDeletableRepository<Rule, int>
{
    Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default);
    Task<Rule> Get(Guid accountId, int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rule>> GetForAccount(Guid accountId, CancellationToken cancellationToken = default);
}
