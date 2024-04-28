using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.InstitutionAccount;

namespace Asm.MooBank.Modules.Accounts.Queries;

public record GetAll() : IQuery<IEnumerable<InstitutionAccount>>;

internal class GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetAll, IEnumerable<InstitutionAccount>>
{
    public async ValueTask<IEnumerable<InstitutionAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = await institutionAccounts.Where(a => a.Owners.Any(ah => ah.UserId == user.Id) || a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId)).ToModel(currencyConverter).ToListAsync(cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == user.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;

    }

}
