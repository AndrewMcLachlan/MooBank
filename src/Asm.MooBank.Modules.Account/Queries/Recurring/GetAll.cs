using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Recurring;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.Recurring;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security, User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        return account.VirtualInstruments.SelectMany(v => v.RecurringTransactions).ToModel();
    }
}
