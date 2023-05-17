using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Account;

public record Get(Guid AccountId) : IQuery<Models.InstitutionAccount>;

internal class GetHandler : IQueryHandler<Get, Models.InstitutionAccount>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts;
    private readonly IUserDataProvider _userDataProvider;
    private readonly ISecurity _security;

    public GetHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IUserDataProvider userDataProvider, ISecurity security)
    {
        _accounts = accounts;
        _userDataProvider = userDataProvider;
        _security = security;
    }

    public async Task<Models.InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await _accounts.Include(a => a.AccountAccountHolders).ThenInclude(ah => ah.AccountGroup).Include(a => a.ImportAccount).Include(a => a.VirtualAccounts).SingleOrDefaultAsync(a => a.AccountId == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        _security.AssertAccountPermission(entity);

        var account = entity.ToModel(_userDataProvider.CurrentUserId);

        return account!;
    }
}
