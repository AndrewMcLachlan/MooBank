namespace Asm.MooBank.Domain.Entities.Instrument;

public interface IInstrumentRepository : IDeletableRepository<Instrument, Guid>
{
    Task<IEnumerable<Instrument>> Get(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
