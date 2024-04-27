using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Rules;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Accounts.Queries.Rule;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Models.Rules.Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security, User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Models.Rules.Rule>>
{
    private readonly IQueryable<Domain.Entities.Account.Instrument> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<IEnumerable<Models.Rules.Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.Include(a => a.Rules).ThenInclude(r => r.Tags).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
