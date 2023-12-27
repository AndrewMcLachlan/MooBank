using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.AccountGroup;

[AggregateRoot]
public class AccountGroup([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public AccountGroup() : this(default) { }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public bool ShowPosition { get; set; }

    public virtual AccountHolder.AccountHolder Owner { get; set; } = null!;

    [NotMapped]
    public virtual ICollection<Account.Account> Accounts { get; set; } = new HashSet<Account.Account>();

}
