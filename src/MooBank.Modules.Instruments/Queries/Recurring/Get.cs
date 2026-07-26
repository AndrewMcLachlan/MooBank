using Asm.MooBank.Modules.Accounts.Models.Recurring;

namespace Asm.MooBank.Modules.Accounts.Queries.Recurring;

public record Get(Guid AccountId, Guid RecurringTransactionId) : IQuery<RecurringTransaction>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts) : IQueryHandler<Get, RecurringTransaction>
{

    public async ValueTask<RecurringTransaction> Handle(Get query, CancellationToken cancellationToken)
    {
        var recurringTransaction = await accounts.Where(a => a.Id == query.AccountId)
            .SelectMany(a => a.VirtualInstruments)
            .SelectMany(v => v.RecurringTransactions)
            .SingleOrDefaultAsync(r => r.Id == query.RecurringTransactionId, cancellationToken);

        return recurringTransaction?.ToModel() ?? throw new NotFoundException();
    }
}
