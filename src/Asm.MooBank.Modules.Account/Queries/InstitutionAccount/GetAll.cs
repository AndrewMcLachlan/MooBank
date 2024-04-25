using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Queries;
using Asm.MooBank.Services;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.InstitutionAccount;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record GetAll() : IQuery<IEnumerable<Models.Account.InstitutionAccount>>;

internal class GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, AccountHolder accountHolder, ICurrencyConverter currencyConverter) : IQueryHandler<GetAll, IEnumerable<Models.Account.InstitutionAccount>>
{
    public async ValueTask<IEnumerable<Models.Account.InstitutionAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = await institutionAccounts.Where(a => a.Owners.Any(ah => ah.UserId == accountHolder.Id) || a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == accountHolder.FamilyId)).ToModel(currencyConverter).ToListAsync(cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == accountHolder.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;

    }

}
