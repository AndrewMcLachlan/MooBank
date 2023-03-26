using System.ComponentModel.DataAnnotations.Schema;
using Asm.Domain;

namespace Asm.MooBank.Domain.Entities.AccountGroup;

[AggregateRoot]
public class AccountGroup : IIdentifiable<Guid>
{

    public AccountGroup()
    {
        Accounts = new HashSet<Account.Account>();
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public bool ShowPosition { get; set; }

    public virtual AccountHolder.AccountHolder Owner { get; set; }

    [NotMapped]
    public virtual ICollection<Account.Account> Accounts { get; set; }

}
