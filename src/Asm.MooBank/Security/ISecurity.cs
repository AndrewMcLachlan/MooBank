
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Security;

public interface ISecurity
{
    void AssertInstrumentPermission(Guid instrumentId);

    Task AssertInstrumentPermissionAsync(Guid instrumentId);

    void AssertInstrumentPermission(Instrument instrument);

    void AssertGroupPermission(Guid groupId);
    void AssertGroupPermission(Group group);

    Task AssertFamilyPermission(Guid familyId);

    Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default);

    Task AssertAdministrator(CancellationToken cancellationToken = default);
}
