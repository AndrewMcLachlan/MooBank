using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Instrument.Events;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstrumentRepository(MooBankContext dataContext, Models.User user) : RepositoryDeleteBase<MooBankContext, Instrument, Guid>(dataContext), IInstrumentRepository
{
    public override Instrument Add(Instrument entity)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new InstrumentCreatedEvent(tracked));
        return tracked;
    }

    public override Instrument Update(Instrument entity)
    {
        var tracked = base.Update(entity);
        tracked.Events.Add(new InstrumentUpdatedEvent(tracked));
        return tracked;
    }

    public override void Delete(Guid id)
    {
        var instrument = Entities.Find(id) ?? throw new NotFoundException();
        instrument.ClosedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public override async Task<IEnumerable<Instrument>> Get(CancellationToken cancellationToken = default)
    {
        var userAccounts = user.Accounts.Concat(user.SharedAccounts);
        return await Entities.Where(i => userAccounts.Contains(i.Id)).ToListAsync(cancellationToken);
    }

    // This load serves system paths that run without a user context, so the Family filter is
    // lifted. The SoftDelete filter stays active: a soft-deleted tag must never load into a rule,
    // or the rules engine would apply it to transactions.
    public override async Task<Instrument> Get(Guid id, CancellationToken cancellationToken = default) =>
        await Entities.Include(a => a.Rules).ThenInclude(a => a.Tags).IgnoreQueryFilters(["Family"]).FindAsync(id, cancellationToken) ?? throw new NotFoundException();

    public async Task<IEnumerable<Instrument>> Get(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await Entities.Where(i => idList.Contains(i.Id)).ToListAsync(cancellationToken);
    }
}
