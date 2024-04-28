using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Security;

public interface ISecurity
{
    void AssertInstrumentPermission(Guid instrumentId);

    Task AssertInstrumentPermissionAsync(Guid instrumentId, CancellationToken cancellationToken);

    void AssertInstrumentPermission(Instrument instrument);

    void AssertGroupPermission(Guid groupId);
    void AssertGroupPermission(Group group);

    Task AssertFamilyPermission(Guid familyId);

    Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default);

    Task AssertAdministrator(CancellationToken cancellationToken = default);
}
