using System.Diagnostics.CodeAnalysis;
using Asm.Drawing;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Tag;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public partial class Tag(int id) : KeyedEntity<int>(id), IEquatable<Tag>
{
    public Tag() : this(default) { }

    [Required]
    [MaxLength(50)]
    [AllowNull]
    public string Name { get; set; }

    public bool Deleted { get; set; }

    public Guid FamilyId { get; set; }

    public HexColour? Colour { get; set; }

    public virtual ICollection<Tag> TaggedTo { get; set; } = new HashSet<Tag>();

    public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

    public virtual TagSettings Settings { get; set; } = new();

    #region Equality
    public bool Equals(Tag? other)
    {
        if (other is null) return false;

        return other.Id == Id;
    }


    public override bool Equals(object? obj) => Equals(obj as Tag);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Tag? left, Tag? right)
    {
        if (left is null) return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(Tag? left, Tag? right) => !(left == right);
    #endregion
}


public class TagEqualityComparer : IEqualityComparer<Tag>
{
    public bool Equals(Tag? x, Tag? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        return x.Id == y.Id;


    }

    public int GetHashCode([DisallowNull] Tag obj) => obj.Id.GetHashCode();
}
