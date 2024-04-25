using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IInstrumentRepository : IDeletableRepository<Instrument, Guid>
{
    Task<InstitutionAccount> GetInstitutionAccount(Guid accountId, CancellationToken cancellationToken);

    Task Reload(InstitutionAccount account, CancellationToken cancellationToken = default);
}
