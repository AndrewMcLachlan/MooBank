using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Domain.Repositories;

public interface ISecurityRepository
{
    void AssertPermission(Guid accountId);
    void AssertPermission(Account account);
}
