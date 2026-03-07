namespace Asm.MooBank.Domain.Entities.Instrument;

public interface IRuleRepository : IDeletableRepository<Rule, int>
{
    Task Delete(Guid instrumentId, int id, CancellationToken cancellationToken = default);
    Task<Rule> Get(Guid instrumentId, int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rule>> GetForInstrument(Guid instrumentId, CancellationToken cancellationToken = default);
}
