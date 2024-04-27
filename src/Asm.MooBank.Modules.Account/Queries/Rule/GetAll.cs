using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Rules;

namespace Asm.MooBank.Modules.Accounts.Queries.Rule;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Models.Rules.Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security) : IQueryHandler<GetAll, IEnumerable<Models.Rules.Rule>>
{
    public async ValueTask<IEnumerable<Models.Rules.Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var account = await accounts.Include(a => a.Rules).ThenInclude(r => r.Tags).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
