using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.Account;

[AggregateRoot]
public abstract class Account(Guid id) : KeyedEntity<Guid>(id)
{
    public Account() : this(Guid.Empty)
    {
    }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal Balance { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    //public DateOnly? LastTransaction { get; set; }

    //public virtual ICollection<Transaction> Transactions { get; set; }

    public virtual ICollection<AccountAccountHolder> AccountAccountHolders { get; set; } = new HashSet<AccountAccountHolder>();

    //public decimal CalculatedBalance { get; set; }

    public virtual ICollection<Rule> Rules { get; set; } = new HashSet<Rule>();

    public AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId)
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
}
