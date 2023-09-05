using Asm.MooBank.Models;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.InstitutionAccount;

namespace Asm.MooBank.Queries.Account;

public record GetAll() : IQuery<IEnumerable<InstitutionAccount>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<InstitutionAccount>>
{
    private readonly IQueryable<DomainInstitutionAccount> _institutionAccounts;

    public GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, AccountHolder accountHolder) : base(accountHolder)
    {
        _institutionAccounts = institutionAccounts;
    }

    public async Task<IEnumerable<InstitutionAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = await _institutionAccounts.Where(a => a.AccountAccountHolders.Any(ah => ah.AccountHolderId == AccountHolder.Id) || (a.ShareWithFamily && a.AccountAccountHolders.Any(ah => ah.AccountHolder.FamilyId == AccountHolder.FamilyId))).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == AccountHolder.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;

    }

}
