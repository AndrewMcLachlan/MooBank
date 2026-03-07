using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Security;

public interface ISecurity
{
    void AssertGroupPermission(Guid groupId);
    void AssertGroupPermission(Group group);

    Task AssertFamilyPermission(Guid familyId);

    Task AssertBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> GetInstrumentIds(CancellationToken cancellationToken = default);

    Task AssertAdministrator(CancellationToken cancellationToken = default);
}
