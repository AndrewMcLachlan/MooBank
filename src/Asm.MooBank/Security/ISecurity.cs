
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Security;

public interface ISecurity
{
    void AssertAccountPermission(Guid accountId);
    void AssertAccountPermission(Instrument account);

    void AssertAccountGroupPermission(Guid accountGroupId);
    void AssertAccountGroupPermission(Group accountGroup);

    Task AssertFamilyPermission(Guid familyId);

    Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetAccountIds(CancellationToken cancellationToken = default);

    Task AssertAdministrator(CancellationToken cancellationToken = default);
}
