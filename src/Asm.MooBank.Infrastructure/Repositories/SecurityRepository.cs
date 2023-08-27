using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Security;


namespace Asm.MooBank.Infrastructure.Repositories;

public class SecurityRepository : ISecurity
{
    private readonly IUserDataProvider _userDataProvider;
    private readonly MooBankContext _mooBankContext;

    public SecurityRepository(MooBankContext dataContext, IUserDataProvider userDataProvider)
    {
        _userDataProvider = userDataProvider;
        _mooBankContext = dataContext;
    }

    public void AssertAccountPermission(Guid accountId)
    {

        if (!_mooBankContext.Accounts.Any(a => a.AccountId == accountId && a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountPermission(Account account)
    {
        if (!account.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId))
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public void AssertAccountGroupPermission(Guid accountGroupId)
    {
        if (!_mooBankContext.AccountGroups.Any(a => a.Id == accountGroupId && a.OwnerId == _userDataProvider.CurrentUserId))
        {
            throw new NotAuthorisedException("Not authorised to view this account group");
        }
    }

    public void AssertAccountGroupPermission(AccountGroup accountGroup)
    {
        if (accountGroup.OwnerId != _userDataProvider.CurrentUserId)
        {
            throw new NotAuthorisedException("Not authorised to view this account");
        }
    }

    public Task<Guid> GetFamilyId(CancellationToken cancellationToken = default) =>
        _mooBankContext.AccountAccountHolder.Where(aah => aah.AccountHolderId == _userDataProvider.CurrentUserId).Select(aah => aah.AccountHolder.FamilyId).Distinct().SingleAsync(cancellationToken);

    public async Task AssetBudgetLinePermission(Guid id, CancellationToken cancellationToken = default)
    {
        var familyId = await GetFamilyId(cancellationToken);

        if (!await _mooBankContext.Set<BudgetLine>().AnyAsync(bl => bl.Budget.FamilyId == familyId && bl.Id == id, cancellationToken))
        {
            throw new NotAuthorisedException("Not authorised to view this budget line");
        }
    }

    public async Task<IEnumerable<Guid>> GetAccountIds(CancellationToken cancellationToken = default) =>
        await _mooBankContext.AccountAccountHolder.Where(aah => aah.AccountHolderId == _userDataProvider.CurrentUserId).Select(aah => aah.AccountId).ToListAsync(cancellationToken);
}
