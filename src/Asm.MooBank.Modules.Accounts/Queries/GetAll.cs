using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using DomainInstitutionAccount = Asm.MooBank.Domain.Entities.Account.LogicalAccount;

namespace Asm.MooBank.Modules.Accounts.Queries;

public record GetAll() : IQuery<IEnumerable<LogicalAccount>>;

internal class GetAllHandler(IQueryable<DomainInstitutionAccount> institutionAccounts, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetAll, IEnumerable<LogicalAccount>>
{
    public async ValueTask<IEnumerable<LogicalAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = await institutionAccounts.Where(a => a.Owners.Any(ah => ah.UserId == user.Id) || a.ShareWithFamily && a.Owners.Any(ah => ah.User.FamilyId == user.FamilyId))
            .ToModelAsync(currencyConverter, cancellationToken);

        var primary = accounts.SingleOrDefault(a => a.Id == user.PrimaryAccountId);

        if (primary != null) primary.IsPrimary = true;

        return accounts;
    }
}
