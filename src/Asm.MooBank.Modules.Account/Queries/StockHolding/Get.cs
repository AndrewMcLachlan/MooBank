using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.StockHolding;

public sealed record Get(Guid Id) : IQuery<Models.Account.StockHolding>;

internal class GetHandler(IQueryable<Domain.Entities.StockHolding.StockHolding> accounts, AccountHolder accountHolder, ISecurity security) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Account.StockHolding>
{
    public async ValueTask<Models.Account.StockHolding> Handle(Get query, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.AccountHolders).ThenInclude(ah => ah.AccountGroup).SingleOrDefaultAsync(a => a.Id == query.Id, cancellationToken) ?? throw new NotFoundException();
        security.AssertAccountPermission(entity);
        var account = entity.ToModel(AccountHolder.Id);

        return account!;
    }
}
