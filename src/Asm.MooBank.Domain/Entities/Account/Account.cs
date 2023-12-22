using System.ComponentModel.DataAnnotations.Schema;
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

    public bool ShareWithFamily { get; set; }

    public virtual ICollection<AccountAccountHolder> AccountHolders { get; set; } = new HashSet<AccountAccountHolder>();

    public virtual ICollection<AccountAccountViewer> AccountViewers { get; set; } = new HashSet<AccountAccountViewer>();


    public virtual ICollection<Rule> Rules { get; set; } = new HashSet<Rule>();

    public virtual AccountGroup.AccountGroup? GetAccountGroup(Guid accountHolderId) =>
        AccountHolders.Where(a => a.AccountHolderId == accountHolderId).Select(aah => aah.AccountGroup).SingleOrDefault();

    public void SetAccountGroup(Guid? accountGroupId, Guid currentUserId)
    {
        var existing = AccountHolders.Single(aah => aah.AccountHolderId == currentUserId);

        existing.AccountGroupId = accountGroupId;
    }

    public void SetAccountHolder(Guid currentUserId)
    {
        var existing = AccountHolders.SingleOrDefault(aah => aah.AccountHolderId == currentUserId);

        if (existing != null) throw new ExistsException("User is already an account holder");

        AccountHolders.Add(new AccountAccountHolder
        {
            AccountHolderId = currentUserId,
        });
    }
}
