using Asm.MooBank.Modules.Instruments.Models.Rules;

namespace Asm.MooBank.Modules.Instruments.Queries.Rules;

public record Get(Guid InstrumentId, int RuleId) : IQuery<Rule>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts) : IQueryHandler<Get, Rule>
{
    public async ValueTask<Rule> Handle(Get request, CancellationToken cancellationToken)
    {
        var account = await accounts.SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        var rule = account.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException();

        return rule.ToModel();
    }
}
