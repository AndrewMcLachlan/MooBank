using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Queries;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.InstitutionAccount;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record GetAll() : IQuery<IEnumerable<Models.Account.InstitutionAccount>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<Models.Account.InstitutionAccount>>
{
    private readonly IQueryable<DomainInstitutionAccount> _institutionAccounts;

    public GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, AccountHolder accountHolder) : base(accountHolder)
    {
        _institutionAccounts = institutionAccounts;
    }

    public async ValueTask<IEnumerable<Models.Account.InstitutionAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = await _institutionAccounts.Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == AccountHolder.Id) || a.ShareWithFamily && a.AccountHolders.Any(ah => ah.AccountHolder.FamilyId == AccountHolder.FamilyId)).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == AccountHolder.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;

    }

}
