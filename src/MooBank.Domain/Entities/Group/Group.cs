using System.Diagnostics.CodeAnalysis;
using Asm.Drawing;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Group;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class Group([DisallowNull] Guid id) : KeyedEntity<Guid>(id)
{
    public Group() : this(default) { }

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public bool ShowPosition { get; set; }

    public HexColour? Colour { get; set; }

    /// <summary>
    /// Where the group sits in its owner's list, lowest first.
    /// </summary>
    /// <remarks>
    /// Only meaningful within one owner's groups; values are not unique across the table.
    /// </remarks>
    public int SortOrder { get; set; }

    public virtual User.User Owner { get; set; } = null!;
}
