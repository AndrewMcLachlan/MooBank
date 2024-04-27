using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Queries.InstitutionAccount;

public record Get(Guid Id) : IQuery<Models.Account.InstitutionAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, User user, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<Get, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .Include(a => a.ImportAccount).Include(a => a.VirtualInstruments).Include(a => a.Institution)
                                   .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken) ?? throw new NotFoundException();

        security.AssertAccountPermission(entity);

        var account = entity.ToModelWithAccountGroup(user, currencyConverter);

        return account!;
    }
}
