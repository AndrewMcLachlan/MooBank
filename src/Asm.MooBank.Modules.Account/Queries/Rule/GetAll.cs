using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.Rule;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Models.Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Account.Account> accounts, ISecurity security, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.Rule>>
{
    private readonly IQueryable<Domain.Entities.Account.Account> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.SingleOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
