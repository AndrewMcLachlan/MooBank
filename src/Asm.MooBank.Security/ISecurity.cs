using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Security;

public interface ISecurity
{
    void AssertAccountPermission(Guid accountId);
    void AssertAccountPermission(Account account);

    void AssertAccountGroupPermission(Guid accountGroupId);
    void AssertAccountGroupPermission(AccountGroup accountGroup);

    Task<Guid> GetFamilyId(CancellationToken cancellationToken = default);
    Task AssetBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetAccountIds(CancellationToken cancellationToken = default);
}
