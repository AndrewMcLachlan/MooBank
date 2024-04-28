using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Domain.Entities.Instrument;

public interface IInstrumentRepository : IDeletableRepository<Instrument, Guid>
{
    Task Reload(Instrument instrument, CancellationToken cancellationToken = default);
}
