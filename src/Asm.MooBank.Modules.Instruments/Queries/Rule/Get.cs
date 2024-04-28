using Asm.MooBank.Modules.Instruments.Queries.Rules;

namespace Asm.MooBank.Modules.Instruments.Queries.Rule;

public record Get(Guid AccountId, int RuleId) : IQuery<Rules.Rule>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts, ISecurity security) : IQueryHandler<Get, Rules.Rule>
{
    public async ValueTask<Rules.Rule> Handle(Get request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var account = await accounts.SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        var rule = account.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException();

        return rule.ToModel();
    }
}
