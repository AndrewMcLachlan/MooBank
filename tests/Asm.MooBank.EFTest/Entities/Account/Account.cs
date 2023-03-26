using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Security;

namespace Asm.MooBank.Domain.Entities.Account;

public class Account
{
    private readonly IUserIdProvider? _userIdProvider;

    public Account()
    {
        //Transactions = new HashSet<Transaction>();
        //AccountHolders = new HashSet<AccountHolder.AccountHolder>();
        //AccountGroups = new HashSet<AccountGroup.AccountGroup>();
        //AccountAccountHolders = new HashSet<AccountAccountHolder>();
    }

    protected Account(IUserIdProvider userIdProvider) : this()
    {
        _userIdProvider = userIdProvider;
    }

    public Guid AccountId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public decimal Balance { get; set; }

    public DateTimeOffset LastUpdated { get; set; }


    /*public virtual IReadOnlyCollection<AccountHolder.AccountHolder> AccountHolders { get; protected set; }

    public virtual IReadOnlyCollection<AccountGroup.AccountGroup> AccountGroups { get; protected set; }*/

    //public virtual ICollection<AccountAccountHolder> AccountAccountHolders { get; set; }

    /*public AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId)
    {
        return AccountAccountHolders.Select(aah => aah.AccountGroup).SingleOrDefault(ag => ag?.OwnerId == accountHolderId);
    }

    public void SetAccountGroup(Guid? accountGroupId, Guid currentUserId)
    {
        var existing = AccountAccountHolders.Single(aah => aah.AccountHolderId == currentUserId);

        existing.AccountGroupId = accountGroupId;
    }

    public void SetAccountHolder(Guid currentUserId)
    {
        var existing = AccountAccountHolders.SingleOrDefault(aah => aah.AccountHolderId == currentUserId);

        if (existing != null) throw new ExistsException("User is already an account holder");

        AccountAccountHolders.Add(new AccountAccountHolder
        {
            AccountHolderId = currentUserId,
        });
    }

    [NotMapped]
    public AccountGroup.AccountGroup? AccountGroup
    {
        get => GetAccountGroup(_userIdProvider?.CurrentUserId ?? Guid.Empty);
    }*/
}
