using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Account.Queries.Rule;

public record Get(Guid AccountId, int RuleId) : IQuery<Models.Rule>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security, MooBank.Models.User accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<Get, Models.Rule>
{
    private readonly IQueryable<Domain.Entities.Account.Instrument> _accounts = accounts;
    private readonly ISecurity _security = security;

    public async ValueTask<Models.Rule> Handle(Get request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);

        var account = await _accounts.SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        var rule = account.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException();

        return rule.ToModel();
    }
}
