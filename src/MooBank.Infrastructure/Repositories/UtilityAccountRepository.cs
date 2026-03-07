using Asm.MooBank.Domain.Entities.Instrument.Events;
using Asm.MooBank.Domain.Entities.Utility;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class UtilityAccountRepository(MooBankContext context, Models.User user) : RepositoryWriteBase<MooBankContext, Account, Guid>(context), IAccountRepository
{
    public override Account Add(Account entity)
    {
        var createdEntity = base.Add(entity);
        createdEntity.Events.Add(new InstrumentCreatedEvent(createdEntity));
        return createdEntity;
    }

    public override Account Update(Account entity)
    {
        var tracked = base.Update(entity);
        tracked.Events.Add(new InstrumentUpdatedEvent(tracked));
        return tracked;
    }

    public override async Task<IEnumerable<Account>> Get(CancellationToken cancellationToken = default) =>
        await Entities
            .Where(a => user.Accounts.Contains(a.Id))
            .Include(a => a.Bills)
            .ToListAsync(cancellationToken);

    public override async Task<Account> Get(Guid id, CancellationToken cancellationToken = default) =>
        await Entities
            .Where(a => a.Id == id && user.Accounts.Contains(a.Id))
            .Include(a => a.Bills)
            .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
}
