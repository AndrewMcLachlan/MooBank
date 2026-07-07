using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Recurring;

namespace Asm.MooBank.Modules.Accounts.Queries.Recurring;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts) : IQueryHandler<GetAll, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetAll query, CancellationToken cancellationToken)
    {
        var recurringTransactions = await accounts.Where(a => a.Id == query.AccountId)
            .SelectMany(a => a.VirtualInstruments)
            .SelectMany(v => v.RecurringTransactions)
            .ToListAsync(cancellationToken);

        return recurringTransactions.ToModel();
    }
}
