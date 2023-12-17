using Asm.MooBank.Modules.Account.Models.Account;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record Get(Guid Id) : IQuery<Models.Account.InstitutionAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IUserDataProvider userDataProvider, ISecurity security) : IQueryHandler<Get, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.AccountAccountHolders).ThenInclude(ah => ah.AccountGroup).Include(a => a.ImportAccount).Include(a => a.VirtualAccounts).Include(a => a.Institution).SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken) ?? throw new NotFoundException();

        security.AssertAccountPermission(entity);

        var account = entity.ToModel(userDataProvider.CurrentUserId);

        return account!;
    }
}
