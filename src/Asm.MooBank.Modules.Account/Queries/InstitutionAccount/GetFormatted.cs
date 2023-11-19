using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;

namespace Asm.MooBank.Modules.Account.Queries.InstitutionAccount;

public record GetFormatted() : IQuery<AccountsList>;

internal class GetFormattedHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, AccountHolder accountHolder) : IQueryHandler<GetFormatted, AccountsList>
{
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts = accounts;
    private readonly AccountHolder _accountHolder = accountHolder;

    public async ValueTask<AccountsList> Handle(GetFormatted request, CancellationToken cancellationToken = default)
    {
        var userId = _accountHolder.Id;

        var accounts = await _accounts.Include(a => a.VirtualAccounts).Include(a => a.AccountAccountHolders).ThenInclude(a => a.AccountGroup)
                                      .Where(a => a.AccountAccountHolders.Any(ah => ah.AccountHolderId == userId) ||
                                                  a.ShareWithFamily && a.AccountAccountHolders.Any(ah => ah.AccountHolder.FamilyId == _accountHolder.FamilyId))
                                      .ToListAsync(cancellationToken);

        var groups = accounts.Select(a => a.GetAccountGroup(userId)).Where(ag => ag != null).Distinct(new IIdentifiableEqualityComparer<Domain.Entities.AccountGroup.AccountGroup, Guid>()!).Select(ag => new AccountListGroup
        {
            Name = ag!.Name,
            Accounts = accounts.Where(a => a.GetAccountGroup(userId)?.Id == ag.Id).ToModel(),
            Position = ag.ShowPosition ? accounts.Where(a => a.GetAccountGroup(userId)?.Id == ag.Id).Sum(a => a.Balance) : null,
        });

        var otherAccounts = new AccountListGroup[] {
            new AccountListGroup
            {
                Name = "Other Accounts",
                Accounts = accounts.Where(a => a.GetAccountGroup(userId) == null).ToModel(),
            }
        };

        return new AccountsList
        {
            AccountGroups = groups.Union(otherAccounts),
            Position = groups.Sum(g => g.Position ?? 0),
        };
    }
}
