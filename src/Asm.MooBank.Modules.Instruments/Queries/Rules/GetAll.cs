using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Rules;

namespace Asm.MooBank.Modules.Instruments.Queries.Rules;

public record GetAll(Guid InstrumentId) : IQuery<IEnumerable<Rule>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts) : IQueryHandler<GetAll, IEnumerable<Rule>>
{
    public async ValueTask<IEnumerable<Rule>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.Rules).ThenInclude(r => r.Tags).SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        return account.Rules.ToModel();
    }
}
