using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IAccountRepository : IDeletableRepository<Account, Guid>
{
    Task<InstitutionAccount> GetInstitutionAccount(Guid accountId, CancellationToken cancellationToken);
}
