using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstrumentRepository(MooBankContext dataContext) : Asm.Domain.Infrastructure.RepositoryDeleteBase<MooBankContext, Instrument, Guid>(dataContext), IInstrumentRepository
{
    public override void Delete(Guid id)
    {
        throw new NotImplementedException();
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
}
