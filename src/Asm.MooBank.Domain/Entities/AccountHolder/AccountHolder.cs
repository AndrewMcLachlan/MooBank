using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.AccountHolder;

[AggregateRoot]
public partial class AccountHolder(Guid id) : KeyedEntity<Guid>(id)
{
    public AccountHolder() : this(default) { }

    public required string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string Currency { get; set; } = "AUD";
    public required Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get;set; }

    [NotMapped]
    public IEnumerable<Account.Account> Accounts => AccountAccountHolders?.Select(a => a.Account) ?? Enumerable.Empty<Account.Account>();

    public virtual ICollection<Account.AccountAccountHolder> AccountAccountHolders { get; set; } = new HashSet<Account.AccountAccountHolder>();

    public virtual ICollection<AccountHolderCard> Cards { get; set; } = new HashSet<AccountHolderCard>();

    public Family.Family Family { get; set; } = null!;
}
