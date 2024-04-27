using Asm.MooBank.Modules.Accounts.Models.Rules;

namespace Asm.MooBank.Modules.Accounts.Queries.Rule;

public record Get(Guid AccountId, int RuleId) : IQuery<Models.Rules.Rule>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security) : IQueryHandler<Get, Models.Rules.Rule>
{
    public async ValueTask<Models.Rules.Rule> Handle(Get request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        var account = await accounts.SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        var rule = account.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException();

        return rule.ToModel();
    }
}
