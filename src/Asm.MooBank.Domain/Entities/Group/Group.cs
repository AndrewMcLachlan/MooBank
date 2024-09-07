using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Group;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class Group([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public Group() : this(default) { }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public bool ShowPosition { get; set; }

    public virtual User.User Owner { get; set; } = null!;

    [NotMapped]
    public virtual ICollection<Instrument.Instrument> Accounts { get; set; } = new HashSet<Instrument.Instrument>();

}
