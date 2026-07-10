using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.Repositories;

public class RuleRepository(MooBankContext context) : RepositoryDeleteBase<Rule, int>(context), IRuleRepository
{
    // Rules run in the background without a user context and deliberately keep their tag
    // references (including soft-deleted tags), so the Tag query filters are lifted here.

    public async Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var entity = await Entities.Include(r => r.Tags).IgnoreQueryFilters().Where(r => r.Id == id && r.InstrumentId == accountId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
        Context.Remove(entity);
    }

    public async Task<Rule> Get(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var rule = await Entities.Include(t => t.Tags).IgnoreQueryFilters().Where(t => t.InstrumentId == accountId && t.Id == id).SingleOrDefaultAsync(cancellationToken);

        return rule ?? throw new NotFoundException("Rule not found");
    }

    public async Task<IEnumerable<Rule>> GetForInstrument(Guid accountId, CancellationToken cancellationToken = default) =>
        await Entities.Include(t => t.Tags).IgnoreQueryFilters().Where(t => t.InstrumentId == accountId).OrderBy(t => t.Contains).ToListAsync(cancellationToken);

    protected override IQueryable<Rule> GetById(int id) => Entities.Where(t => t.Id == id);

}
