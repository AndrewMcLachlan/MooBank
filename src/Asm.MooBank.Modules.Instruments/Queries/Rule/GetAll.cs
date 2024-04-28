using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Queries.Rules;

namespace Asm.MooBank.Modules.Instruments.Queries.Rule;

public record GetAll(Guid AccountId) : IQuery<IEnumerable<Rules.Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts, ISecurity security) : IQueryHandler<GetAll, IEnumerable<Rules.Rule>>
{
    public async ValueTask<IEnumerable<Rules.Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var account = await accounts.Include(a => a.Rules).ThenInclude(r => r.Tags).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
