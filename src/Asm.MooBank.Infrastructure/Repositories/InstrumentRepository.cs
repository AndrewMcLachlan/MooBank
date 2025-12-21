using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstrumentRepository(MooBankContext dataContext, Models.User user) : RepositoryDeleteBase<MooBankContext, Instrument, Guid>(dataContext), IInstrumentRepository
{
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

    public override Task<Instrument> Get(Guid id, CancellationToken cancellationToken = default) =>
        (Entities.Include(a => a.Rules).ThenInclude(a => a.Tags).FindAsync(id, cancellationToken) ?? throw new NotFoundException())!;

    public async Task<LogicalAccount> GetInstitutionAccount(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await GetById(accountId).Include("VirtualAccounts").SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (account is not LogicalAccount institutionAccount)
            throw new InvalidOperationException("Cannot update virtual account on non-institution account.");

        return institutionAccount;
    }

    protected IQueryable<Instrument> GetById(Guid id) => Entities.Where(a => a.Id == id);

    public Task Reload(Instrument instrument, CancellationToken cancellationToken) =>
        Context.Entry(instrument).ReloadAsync(cancellationToken);

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
