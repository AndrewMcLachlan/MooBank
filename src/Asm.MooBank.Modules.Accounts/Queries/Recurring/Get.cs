using Asm.MooBank.Modules.Accounts.Models.Recurring;

namespace Asm.MooBank.Modules.Accounts.Queries.Recurring;

public record Get(Guid AccountId, Guid RecurringTransactionId) : IQuery<RecurringTransaction>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts, ISecurity security) : IQueryHandler<Get, RecurringTransaction>
{

    public async ValueTask<RecurringTransaction> Handle(Get query, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(query.AccountId);

        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        return account.VirtualInstruments.SelectMany(v => v.RecurringTransactions).SingleOrDefault(r => r.Id == query.RecurringTransactionId)?.ToModel() ?? throw new NotFoundException();
    }
}
