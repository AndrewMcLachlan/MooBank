using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Domain.Entities.AccountHolder;

public partial class AccountHolder
{
    public AccountHolder()
    {
        AccountAccountHolders = new HashSet<Account.AccountAccountHolder>();
        Cards = new HashSet<AccountHolderCard>();
    }
    public required Guid AccountHolderId { get; set; }
    public required string EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public required Guid FamilyId { get; set; }

    public Guid? PrimaryAccountId { get;set; }

    [NotMapped]
    public IEnumerable<Account.Account> Accounts => AccountAccountHolders?.Select(a => a.Account) ?? Enumerable.Empty<Account.Account>();

    public virtual ICollection<Account.AccountAccountHolder> AccountAccountHolders { get; set; }

    public virtual ICollection<AccountHolderCard> Cards { get; set; }

    public Family.Family Family { get; set; } = null!;
}
