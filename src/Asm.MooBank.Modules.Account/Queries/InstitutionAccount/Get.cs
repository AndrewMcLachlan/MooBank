using Asm.MooBank.Modules.Account.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record Get(Guid Id) : IQuery<Models.Account.InstitutionAccount>;

internal class GetHandler : IQueryHandler<Get, Models.Account.InstitutionAccount>
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

    public async ValueTask<Models.Account.InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await _accounts.Include(a => a.AccountAccountHolders).ThenInclude(ah => ah.AccountGroup).Include(a => a.ImportAccount).Include(a => a.VirtualAccounts).Include(a => a.Institution).SingleOrDefaultAsync(a => a.AccountId == request.Id, cancellationToken) ?? throw new NotFoundException();

        _security.AssertAccountPermission(entity);

        var account = entity.ToModel(_userDataProvider.CurrentUserId);

        return account!;
    }
}
