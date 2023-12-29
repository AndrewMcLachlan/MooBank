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
        var accounts = await institutionAccounts.Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == accountHolder.Id) || a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == accountHolder.FamilyId)).ToModel(currencyConverter).ToListAsync(cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == accountHolder.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;

    }

}
