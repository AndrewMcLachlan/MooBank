using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record Get(Guid Id) : IQuery<Models.Account.InstitutionAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, AccountHolder accountHolder, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<Get, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.AccountHolders).ThenInclude(ah => ah.AccountGroup)
                                   .Include(a => a.AccountHolders).ThenInclude(ah => ah.AccountHolder)
                                   .Include(a => a.AccountViewers).ThenInclude(ah => ah.AccountGroup)
                                   .Include(a => a.AccountViewers).ThenInclude(ah => ah.AccountHolder)
                                   .Include(a => a.ImportAccount).Include(a => a.VirtualAccounts).Include(a => a.Institution)
                                   .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken) ?? throw new NotFoundException();

        security.AssertAccountPermission(entity);

        var account = entity.ToModelWithAccountGroup(accountHolder, currencyConverter);

        return account!;
    }
}
