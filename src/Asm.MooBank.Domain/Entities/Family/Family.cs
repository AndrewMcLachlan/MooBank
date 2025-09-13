using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Family;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class Family : KeyedEntity<Guid>
{
    public Family() : base(Guid.Empty)
    {
    }

    public Family([DisallowNull] Guid id) : base(id)
    {
    }

    public required string Name { get; set; }

    public virtual ICollection<User.User> AccountHolders { get; set; } = [];
}
