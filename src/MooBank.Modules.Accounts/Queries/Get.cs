using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Accounts.Queries;

public record Get(Guid InstrumentId) : IQuery<Models.Account.LogicalAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.LogicalAccount> accounts, User user, ICurrencyConverter currencyConverter) : IQueryHandler<Get, Models.Account.LogicalAccount>
{
    public async ValueTask<Models.Account.LogicalAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var entity = await accounts.Include(a => a.Owners).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Owners).ThenInclude(ah => ah.User)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.Group)
                                   .Include(a => a.Viewers).ThenInclude(ah => ah.User)
                                   .Include(a => a.InstitutionAccounts)
                                   .SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        var account = entity.ToModelWithGroup(user, currencyConverter);

        return account!;
    }
}
