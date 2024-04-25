using Asm.MooBank.Modules.Account.Models.Recurring;

namespace Asm.MooBank.Modules.Account.Queries.Recurring;

public record GetForVirtual(Guid AccountId, Guid VirtualAccountId) : IQuery<IEnumerable<RecurringTransaction>>;

internal class GetForVirtualHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security) : IQueryHandler<GetForVirtual, IEnumerable<RecurringTransaction>>
{
    public async ValueTask<IEnumerable<RecurringTransaction>> Handle(GetForVirtual query, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(query.AccountId);

        var account = await accounts.Include(a => a.VirtualAccounts).ThenInclude(a => a.RecurringTransactions).SingleAsync(a => a.Id == query.AccountId, cancellationToken);

        var virtualAccount = account.VirtualAccounts.SingleOrDefault(v => v.Id == query.VirtualAccountId) ?? throw new NotFoundException();

        return virtualAccount.RecurringTransactions.ToModel();
    }
}
