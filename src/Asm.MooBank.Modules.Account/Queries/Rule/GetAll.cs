using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.Rule;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Models.Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.Rule>>
{
    private readonly IQueryable<Domain.Entities.Account.Instrument> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.Rules).ThenInclude(r => r.Tags).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
