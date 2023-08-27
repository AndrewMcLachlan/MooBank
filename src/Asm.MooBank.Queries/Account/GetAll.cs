using Asm.MooBank.Models;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.InstitutionAccount;

namespace Asm.MooBank.Queries.Account;

public record GetAll() : IQuery<IEnumerable<InstitutionAccount>>;

internal class GetAllHandler : IQueryHandler<GetAll, IEnumerable<InstitutionAccount>>
{
    private readonly IQueryable<DomainInstitutionAccount> _institutionAccounts;
    private readonly IUserDataProvider _userDataProvider;

    public GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, IUserDataProvider userDataProvider)
    {
        _institutionAccounts = institutionAccounts;
        _userDataProvider = userDataProvider;
    }

    public Task<IEnumerable<InstitutionAccount>> Handle(GetAll request, CancellationToken cancellationToken) =>
        _institutionAccounts.Where(a => a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)).ToListAsync(cancellationToken).ToModelAsync(cancellationToken);
}
