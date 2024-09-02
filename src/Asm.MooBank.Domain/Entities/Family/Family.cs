using System.Diagnostics.CodeAnalysis;
using Asm.Domain;
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

    public string Name { get; set; } = null!;

    public virtual ICollection<User.User> AccountHolders { get; set; } = new List<User.User>();
}
