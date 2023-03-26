using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Domain.Repositories;

public interface ISecurityRepository
{
    void AssertAccountPermission(Guid accountId);
    void AssertAccountPermission(Account account);

    void AssertAccountGroupPermission(Guid accountGroupId);
    void AssertAccountGroupPermission(AccountGroup accountGroup);
}
